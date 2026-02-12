using GitHub.Copilot.SDK;

namespace Microsoft.Extensions.Hosting;

/// <summary>
/// Factory for creating per-user <see cref="CopilotClient"/> instances.
/// </summary>
public class CopilotClientFactory(string cliUrl)
{
    /// <summary>
    /// Creates a <see cref="CopilotClient"/> authenticated with the given GitHub token.
    /// </summary>
    public CopilotClient Create(string githubToken)
    {
        return new CopilotClient(new CopilotClientOptions
        {
            CliUrl = cliUrl,
            UseStdio = false,
            GithubToken = githubToken,
            UseLoggedInUser = false,
        });
    }
}

/// <summary>
/// Extension methods for registering the Copilot SDK client via Aspire.
/// </summary>
public static class CopilotClientExtensions
{
    /// <summary>
    /// Adds a <see cref="CopilotClientFactory"/> to the service collection,
    /// configured from Aspire service discovery for the named resource.
    /// </summary>
    public static void AddCopilotClient(this IHostApplicationBuilder builder, string serviceName = "copilot")
    {
        var url = builder.Configuration[$"services:{serviceName}:http:0"]
            ?? throw new InvalidOperationException(
                $"Service endpoint '{serviceName}' not found. Ensure the Copilot CLI resource is referenced in the AppHost.");

        var uri = new Uri(url);
        var cliUrl = $"{uri.Host}:{uri.Port}";

        builder.Services.AddSingleton(new CopilotClientFactory(cliUrl));
    }
}
