using Aspire.Hosting.ApplicationModel;

namespace Aspire.Hosting.ApplicationModel;

/// <summary>
/// Represents the Copilot CLI running in server mode.
/// </summary>
/// <param name="name">The resource name.</param>
public class CopilotCliResource(string name)
    : ExecutableResource(name, "copilot", "."), IResourceWithConnectionString
{
    /// <summary>
    /// Gets a reference to the HTTP endpoint exposed by the Copilot CLI server.
    /// </summary>
    public EndpointReference HttpEndpoint => new(this, "http");

    /// <summary>
    /// Gets the connection string expression for the Copilot CLI server (the HTTP URL).
    /// </summary>
    public ReferenceExpression ConnectionStringExpression =>
        ReferenceExpression.Create($"{HttpEndpoint.Property(EndpointProperty.Url)}");
}
