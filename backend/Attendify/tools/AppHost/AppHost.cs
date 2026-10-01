using Aspire.Hosting.Yarp.Transforms;
using Projects;

#pragma warning disable ASPIRECOMPUTE003

var builder = DistributedApplication.CreateBuilder(args);

var resendApiKey = builder.AddParameter("ResendApiKey", secret: true);
var emailFromAddress = builder.AddParameter("EmailFromAddress");
var emailFromName = builder.AddParameter("EmailFromName");
var passwordResetCodeHashKey = builder.AddParameter("PasswordResetCodeHashKey", secret: true);
var studentAccessCodeSecret = builder.AddParameter("StudentAccessCodeGenSecret", secret: true);

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
    .WithEnvironment("Resend__ApiKey", resendApiKey)
    .WithEnvironment("Email__FromAddress", emailFromAddress)
    .WithEnvironment("Email__FromName", emailFromName)
    .WithEnvironment("ResetPasswordToken__SecurityCodeHashKey", passwordResetCodeHashKey)
    .WithEnvironment("Parameters__StudentAccessCodeGenSecret", studentAccessCodeSecret)
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
    .WithContainerRegistry(registry)
    .WaitFor(api)
    .WithExternalHttpEndpoints();

var gateway = builder.AddYarp("gateway")
    .WithHttpEndpoint(port: 80, targetPort: 5000, name: "http")
    .WithConfiguration(yarp =>
    {
        yarp.AddRoute("/api/{**catch-all}", api);

        if (builder.ExecutionContext.IsRunMode)
            yarp.AddRoute("{**catch-all}", webapp);
    }).PublishWithStaticFiles(webapp);

gateway.WithCloudflareTunnel(tunnel, hostname: hostname);

builder.Build().Run();
