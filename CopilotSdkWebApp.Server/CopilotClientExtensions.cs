using GitHub.Copilot.SDK;

namespace Microsoft.Extensions.Hosting;

/// <summary>
/// Extension methods for registering the Copilot SDK client via Aspire.
/// </summary>
public static class CopilotClientExtensions
{
    /// <summary>
    /// Adds a <see cref="CopilotClient"/> to the service collection,
    /// configured from Aspire service discovery for the named resource.
    /// </summary>
    /// <param name="builder">The host application builder.</param>
    /// <param name="serviceName">The Aspire resource name (default: "copilot").</param>
    public static void AddCopilotClient(this IHostApplicationBuilder builder, string serviceName = "copilot")
    {
        builder.Services.AddSingleton(sp =>
        {
            // Aspire injects the resolved endpoint as services:{name}:{scheme}:{index} in configuration
            var url = builder.Configuration[$"services:{serviceName}:http:0"]
                ?? throw new InvalidOperationException(
                    $"Service endpoint '{serviceName}' not found. Ensure the Copilot CLI resource is referenced in the AppHost.");

            var uri = new Uri(url);
            var cliUrl = $"{uri.Host}:{uri.Port}";

            return new CopilotClient(new CopilotClientOptions
            {
                CliUrl = cliUrl,
                UseStdio = false,
            });
        });
    }
}
