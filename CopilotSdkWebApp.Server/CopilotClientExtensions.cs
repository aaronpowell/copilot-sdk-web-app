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
        builder.Services.AddHttpClient(serviceName, client =>
        {
            client.BaseAddress = new Uri($"http://{serviceName}");
        });

        builder.Services.AddSingleton(sp =>
        {
            var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
            var httpClient = httpClientFactory.CreateClient(serviceName);
            var cliUrl = $"{httpClient.BaseAddress!.Host}:{httpClient.BaseAddress.Port}";

            return new CopilotClient(new CopilotClientOptions
            {
                CliUrl = cliUrl,
            });
        });
    }
}
