using GitHub.Copilot.SDK;

namespace Microsoft.Extensions.Hosting;

/// <summary>
/// Factory for creating per-user <see cref="CopilotClient"/> instances.
/// Each client spawns its own Copilot CLI process authenticated with the user's token.
/// </summary>
public class CopilotClientFactory
{
    /// <summary>
    /// Creates a <see cref="CopilotClient"/> authenticated with the given GitHub token.
    /// </summary>
    public CopilotClient Create(string githubToken)
    {
        return new CopilotClient(new CopilotClientOptions
        {
            GitHubToken = githubToken,
            UseLoggedInUser = false,
        });
    }
}

/// <summary>
/// Extension methods for registering the Copilot SDK client factory.
/// </summary>
public static class CopilotClientExtensions
{
    /// <summary>
    /// Adds a <see cref="CopilotClientFactory"/> to the service collection.
    /// </summary>
    public static void AddCopilotClient(this IHostApplicationBuilder builder)
    {
        builder.Services.AddSingleton<CopilotClientFactory>();
    }
}
