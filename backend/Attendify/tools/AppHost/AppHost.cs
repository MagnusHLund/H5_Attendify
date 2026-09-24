using Projects;

var builder = DistributedApplication.CreateBuilder(args);

var tunnelName = builder.Configuration.GetSection("Parameters")["CloudflareTunnelName"];
if (string.IsNullOrWhiteSpace(tunnelName))
    throw new InvalidOperationException("CloudflareTunnelName is required.");

var tunnel = builder.AddCloudflareTunnel(tunnelName);

var hostname = builder.Configuration.GetSection("Parameters")["Hostname"];
if (string.IsNullOrWhiteSpace(hostname))
    throw new InvalidOperationException("Hostname is required.");

var jwtSigningKey = builder.AddParameter("SigningKey", secret: true);
var facialEmbeddingEncryptionKey = builder.AddParameter(
    "FacialEmbeddingEncryptionKey",
    secret: true
);

var postgres = builder
    .AddPostgres("postgres")
    .WithDataVolume()
    .WithLifetime(ContainerLifetime.Persistent);

var db = postgres.AddDatabase("AppDb", "app-db");

var migrationService = builder
    .AddProject<MigrationService>("migrations")
    .WithReference(db)
    .WaitFor(postgres);

var api = builder
    .AddProject<WebApi>("api")
    .WithExternalHttpEndpoints()
    .WithReference(db)
    .WithEnvironment("Jwt__SigningKey", jwtSigningKey)
    .WithEnvironment("FacialEmbedding__EncryptionKey", facialEmbeddingEncryptionKey)
    .WaitForCompletion(migrationService);

var webapp = builder
    .AddViteApp("webapp", "../../../../webapp/Attendify")
    .WithReference(api)
    .WaitFor(api)
    .WithExternalHttpEndpoints();

var gateway = builder
    .AddYarp("gateway")
    .WithConfiguration(yarp =>
    {
        yarp.AddRoute("/api/{**catch-all}", api);
        yarp.AddRoute("{**catch-all}", webapp);
    });

gateway.WithCloudflareTunnel(tunnel, hostname: hostname);

builder.Build().Run();
