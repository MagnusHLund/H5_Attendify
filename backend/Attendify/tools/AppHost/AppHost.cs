using Projects;

var builder = DistributedApplication.CreateBuilder(args);

var controllerName = builder.AddParameter("PrivacyPolicyControllerName");
var controllerAddress = builder.AddParameter("PrivacyPolicyControllerAddress");
var controllerEmail = builder.AddParameter("PrivacyPolicyControllerEmail");
var dpoContact = builder.AddParameter("PrivacyPolicyDpoContact");
var authorityName = builder.AddParameter("PrivacyPolicyAuthorityName");
var authorityUrl = builder.AddParameter("PrivacyPolicyAuthorityUrl");

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
    .WithEnvironment("VITE_PRIVACY_POLICY_CONTROLLER_NAME", controllerName)
    .WithEnvironment("VITE_PRIVACY_POLICY_CONTROLLER_ADDRESS", controllerAddress)
    .WithEnvironment("VITE_PRIVACY_POLICY_CONTROLLER_EMAIL", controllerEmail)
    .WithEnvironment("VITE_PRIVACY_POLICY_DPO_CONTACT", dpoContact)
    .WithEnvironment("VITE_PRIVACY_POLICY_AUTHORITY_NAME", authorityName)
    .WithEnvironment("VITE_PRIVACY_POLICY_AUTHORITY_URL", authorityUrl)
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
