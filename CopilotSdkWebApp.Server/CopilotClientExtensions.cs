using GitHub.Copilot.SDK;

namespace Microsoft.Extensions.Hosting;

/// <summary>
/// Extension methods for registering the Copilot SDK client via Aspire.
/// </summary>
public static class CopilotClientExtensions
{
    /// <summary>
    /// Adds a <see cref="CopilotClient"/> to the service collection,
    /// configured from the Aspire connection string for the named resource.
    /// </summary>
    /// <param name="builder">The host application builder.</param>
    /// <param name="connectionName">The Aspire resource connection name (default: "copilot").</param>
    public static void AddCopilotClient(this IHostApplicationBuilder builder, string connectionName = "copilot")
    {
        builder.Services.AddSingleton(sp =>
        {
            var connectionString = builder.Configuration.GetConnectionString(connectionName)
                ?? throw new InvalidOperationException(
                    $"Connection string '{connectionName}' not found. Ensure the Copilot CLI resource is referenced in the AppHost.");

            // The connection string is a full URL (e.g. http://localhost:12345).
            // The SDK expects host:port format for CliUrl.
            var uri = new Uri(connectionString);
            var cliUrl = $"{uri.Host}:{uri.Port}";

            return new CopilotClient(new CopilotClientOptions
            {
                CliUrl = cliUrl,
            });
        });
    }
}
