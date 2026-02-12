using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OAuth;

var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire client integrations.
builder.AddServiceDefaults();
builder.AddCopilotClient();

// Add services to the container.
builder.Services.AddProblemDetails();
builder.Services.AddOpenApi();

// Configure GitHub OAuth authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = "GitHub";
})
.AddCookie(options =>
{
    options.LoginPath = "/auth/login";
    options.LogoutPath = "/auth/logout";
    options.Events.OnRedirectToLogin = context =>
    {
        context.Response.StatusCode = 401;
        return Task.CompletedTask;
    };
})
.AddOAuth("GitHub", options =>
{
    options.ClientId = builder.Configuration["GitHub:ClientId"] ?? throw new InvalidOperationException("GitHub:ClientId is required");
    options.ClientSecret = builder.Configuration["GitHub:ClientSecret"] ?? throw new InvalidOperationException("GitHub:ClientSecret is required");
    options.AuthorizationEndpoint = "https://github.com/login/oauth/authorize";
    options.TokenEndpoint = "https://github.com/login/oauth/access_token";
    options.UserInformationEndpoint = "https://api.github.com/user";
    options.CallbackPath = "/auth/callback";
    options.Scope.Add("copilot");
    options.SaveTokens = true;

    options.ClaimActions.MapJsonKey(ClaimTypes.NameIdentifier, "id");
    options.ClaimActions.MapJsonKey(ClaimTypes.Name, "login");
    options.ClaimActions.MapJsonKey("urn:github:avatar", "avatar_url");

    options.Events = new OAuthEvents
    {
        OnCreatingTicket = async context =>
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, context.Options.UserInformationEndpoint);
            request.Headers.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", context.AccessToken);

            using var response = await context.Backchannel.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, context.HttpContext.RequestAborted);
            response.EnsureSuccessStatusCode();

            var user = await response.Content.ReadFromJsonAsync<System.Text.Json.JsonElement>();
            context.RunClaimActions(user);
        }
    };
});

builder.Services.AddAuthorization();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseAuthentication();
app.UseAuthorization();

// Auth endpoints
app.MapGet("/auth/login", (HttpContext context) =>
    Results.Challenge(new AuthenticationProperties { RedirectUri = "/" }, ["GitHub"]));

app.MapPost("/auth/logout", async (HttpContext context) =>
{
    await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    return Results.Ok();
});

app.MapGet("/auth/user", (HttpContext context) =>
{
    if (context.User.Identity?.IsAuthenticated != true)
        return Results.Json(new { authenticated = false });

    return Results.Json(new
    {
        authenticated = true,
        name = context.User.FindFirstValue(ClaimTypes.Name),
        avatar = context.User.FindFirstValue("urn:github:avatar")
    });
});

// API endpoints (require auth)
var api = app.MapGroup("/api").RequireAuthorization();

api.MapPost("chat", async (ChatRequest request, CopilotClientFactory factory, HttpContext context) =>
{
    var token = await context.GetTokenAsync("access_token")
        ?? throw new InvalidOperationException("No access token found");

    await using var copilotClient = factory.Create(token);
    await using var session = await copilotClient.CreateSessionAsync(new GitHub.Copilot.SDK.SessionConfig
    {
        Model = request.Model ?? "gpt-5"
    });

    var done = new TaskCompletionSource();
    var responseContent = "";

    session.On(evt =>
    {
        if (evt is GitHub.Copilot.SDK.AssistantMessageEvent msg)
        {
            responseContent = msg.Data.Content;
        }
        else if (evt is GitHub.Copilot.SDK.SessionIdleEvent)
        {
            done.TrySetResult();
        }
        else if (evt is GitHub.Copilot.SDK.SessionErrorEvent err)
        {
            done.TrySetException(new Exception(err.Data.Message));
        }
    });

    await session.SendAsync(new GitHub.Copilot.SDK.MessageOptions { Prompt = request.Message });
    await done.Task;

    return TypedResults.Ok(new ChatResponse(responseContent));
})
.WithName("Chat");

api.MapGet("models", async (CopilotClientFactory factory, HttpContext context) =>
{
    var token = await context.GetTokenAsync("access_token")
        ?? throw new InvalidOperationException("No access token found");

    await using var copilotClient = factory.Create(token);
    var models = await copilotClient.ListModelsAsync();
    return TypedResults.Ok(models);
})
.WithName("ListModels");

app.MapDefaultEndpoints();

app.UseFileServer();

app.Run();

record ChatRequest(string Message, string? Model = null);
record ChatResponse(string Reply);
