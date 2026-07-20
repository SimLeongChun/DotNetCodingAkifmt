var builder = DistributedApplication.CreateBuilder(args);

var apiService = builder.AddProject<Projects.AspireKeycloakExternalService_ApiService>("apiservice")
    .WithHttpHealthCheck("/health");

builder.AddProject<Projects.AspireKeycloakExternalService_Web>("webfrontend")
    .WithExternalHttpEndpoints()
    .WithHttpHealthCheck("/health")
    .WithReference(apiService)
    .WaitFor(apiService);

builder.AddExternalService("keycloak", "http://localhost:9000")
    .WithUrl("http://localhost:8080")
    .WithHttpHealthCheck("/health")
    .WithExplicitStart();


builder.Build().Run();
