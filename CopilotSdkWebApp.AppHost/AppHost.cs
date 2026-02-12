var builder = DistributedApplication.CreateBuilder(args);

var copilot = builder.AddCopilotCli("copilot");

var server = builder.AddProject<Projects.CopilotSdkWebApp_Server>("server")
    .WithHttpHealthCheck("/health")
    .WithExternalHttpEndpoints()
    .WithReference(copilot)
    .WaitFor(copilot);

var webfrontend = builder.AddViteApp("webfrontend", "../frontend")
    .WithReference(server)
    .WaitFor(server);

server.PublishWithContainerFiles(webfrontend, "wwwroot");

builder.Build().Run();
