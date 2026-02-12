var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire client integrations.
builder.AddServiceDefaults();
builder.AddCopilotClient();

// Add services to the container.
builder.Services.AddProblemDetails();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}


var api = app.MapGroup("/api");

api.MapPost("chat", async (ChatRequest request, GitHub.Copilot.SDK.CopilotClient copilotClient) =>
{
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

api.MapGet("models", async (GitHub.Copilot.SDK.CopilotClient copilotClient) =>
{
    var models = await copilotClient.ListModelsAsync();
    return TypedResults.Ok(models);
})
.WithName("ListModels");

app.MapDefaultEndpoints();

app.UseFileServer();

app.Run();

record ChatRequest(string Message, string? Model = null);
record ChatResponse(string Reply);
