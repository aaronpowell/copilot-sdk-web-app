using Aspire.Hosting.ApplicationModel;

namespace Aspire.Hosting.ApplicationModel;

/// <summary>
/// Represents the Copilot CLI running in server mode.
/// </summary>
/// <param name="name">The resource name.</param>
public class CopilotCliResource(string name)
    : ExecutableResource(name, "copilot", "."), IResourceWithEndpoints, IResourceWithConnectionString
{
    /// <summary>
    /// Gets a reference to the HTTP endpoint exposed by the Copilot CLI server.
    /// </summary>
    public EndpointReference HttpEndpoint => new(this, "http");

    public ReferenceExpression ConnectionStringExpression => ReferenceExpression.Create($"Endpoint={HttpEndpoint}");
}
