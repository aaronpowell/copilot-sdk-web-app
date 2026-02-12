var builder = DistributedApplication.CreateBuilder(args);

var githubClientId = builder.AddParameter("github-client-id");
var githubClientSecret = builder.AddParameter("github-client-secret", secret: true);

var copilot = builder.AddCopilotCli("copilot");

var server = builder.AddProject<Projects.CopilotSdkWebApp_Server>("server")
    .WithHttpHealthCheck("/health")
    .WithExternalHttpEndpoints()
    .WithReference(copilot)
    .WaitFor(copilot)
    .WithEnvironment("GitHub__ClientId", githubClientId)
    .WithEnvironment("GitHub__ClientSecret", githubClientSecret);

var webfrontend = builder.AddViteApp("webfrontend", "../frontend")
    .WithReference(server)
    .WaitFor(server);

server.PublishWithContainerFiles(webfrontend, "wwwroot");

builder.Build().Run();
