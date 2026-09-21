using Aspire.Hosting.Yarp.Transforms;
using Projects;

#pragma warning disable ASPIRECOMPUTE003

var builder = DistributedApplication.CreateBuilder(args);

var tunnel = builder.AddCloudflareTunnel("lunnel");

var hostname = builder.Configuration.GetSection("Parameters")["Hostname"];
if (string.IsNullOrWhiteSpace(hostname))
    throw new InvalidOperationException("Hostname is required.");

var jwtSigningKey = builder.AddParameter("SigningKey", secret: true);
var facialEmbeddingEncryptionKey = builder.AddParameter(
    "FacialEmbeddingEncryptionKey",
    secret: true
);

var endpoint = builder.AddParameter("registry-endpoint");
var repository = builder.AddParameter("registry-repository");
var registry = builder.AddContainerRegistry("registry", endpoint, repository);
var postgresPassword = builder.AddParameter(
    "postgres-password",
    secret: true);

var k8s = builder.AddKubernetesEnvironment("k8s")
    .WithContainerRegistry(registry);

var postgres = builder
    .AddPostgres("postgres",
        password: postgresPassword)
    .WithDataVolume()
    .WithLifetime(ContainerLifetime.Persistent);

var db = postgres.AddDatabase("AppDb", "app-db");

var migrationService = builder
    .AddProject<MigrationService>("migrations")
    .WithContainerRegistry(registry)
    .WithReference(db)
    .WaitFor(postgres);

var api = builder
    .AddProject<WebApi>("api")
    .WithContainerRegistry(registry)
    .WithExternalHttpEndpoints()
    .WithReference(db)
    .WithEnvironment("Jwt__SigningKey", jwtSigningKey)
    .WithEnvironment("FacialEmbedding__EncryptionKey", facialEmbeddingEncryptionKey)
    .WaitForCompletion(migrationService);

var webapp = builder
    .AddViteApp("webapp", "../../../../webapp/Attendify")
    .WithReference(api)
    .WithContainerRegistry(registry)
    .WaitFor(api)
    .WithExternalHttpEndpoints();

var gateway = builder.AddYarp("gateway")
    .WithHttpEndpoint(port: 80, targetPort: 5000, name: "http")
    .WithConfiguration(yarp =>
    {
        yarp.AddRoute("/api/{**catch-all}", api);
        yarp.AddRoute("{**catch-all}", webapp);
    }).PublishWithStaticFiles(webapp);

gateway.WithCloudflareTunnel(tunnel, hostname: hostname);

builder.Build().Run();