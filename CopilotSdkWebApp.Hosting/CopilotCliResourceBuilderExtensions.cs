using Aspire.Hosting.ApplicationModel;

namespace Aspire.Hosting;

/// <summary>
/// Extension methods for adding the Copilot CLI server to an Aspire application.
/// </summary>
public static class CopilotCliResourceBuilderExtensions
{
    /// <summary>
    /// Adds a Copilot CLI server resource to the application.
    /// </summary>
    /// <param name="builder">The distributed application builder.</param>
    /// <param name="name">The resource name.</param>
    /// <param name="port">Optional fixed port. If not specified, a port will be allocated.</param>
    /// <returns>A resource builder for further configuration.</returns>
    public static IResourceBuilder<CopilotCliResource> AddCopilotCli(
        this IDistributedApplicationBuilder builder,
        [ResourceName] string name,
        int? port = null)
    {
        var resource = new CopilotCliResource(name);

        return builder.AddResource(resource)
            .WithHttpEndpoint(port: port, targetPort: port, name: "http")
            .WithArgs(context =>
            {
                context.Args.Add("--server");
                context.Args.Add("--port");
                context.Args.Add(resource.GetEndpoint("http").Property(EndpointProperty.TargetPort));
            });
    }
}
