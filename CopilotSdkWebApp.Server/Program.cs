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


string[] summaries = ["Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"];

var api = app.MapGroup("/api");
api.MapGet("weatherforecast", () =>
{
    var forecast = Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast");

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

app.MapDefaultEndpoints();

app.UseFileServer();

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}

record ChatRequest(string Message, string? Model = null);
record ChatResponse(string Reply);
