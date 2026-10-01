using Aspire.Hosting.ApplicationModel;
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
var legalBasisDetails = builder.AddParameter("PrivacyPolicyLegalBasisDetails");
var biometricConditionDetails = builder.AddParameter("PrivacyPolicyBiometricConditionDetails");
var retentionDetails = builder.AddParameter("PrivacyPolicyRetentionDetails");
var recipientsDetails = builder.AddParameter("PrivacyPolicyRecipientsDetails");
var transferDetails = builder.AddParameter("PrivacyPolicyTransferDetails");
var dpiaDetails = builder.AddParameter("PrivacyPolicyDpiaDetails");
var attendanceEventRetentionDays = builder.AddParameter("AttendanceEventRetentionDays");

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
var postgresPassword = builder.AddParameter("postgres-password", secret: true);

var k8s = builder.AddKubernetesEnvironment("k8s").WithContainerRegistry(registry);

var postgresData = k8s.AddPersistentVolume("postgres-data")
    .WithStorageClass("local-path")
    .WithCapacity("20Gi");

var postgres = builder
    .AddPostgres("postgres",
        password: postgresPassword)
    .WithDataVolume("postgres-data")
    .WithPersistentVolume(postgresData)
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
    .WithEnvironment("DataRetention__AttendanceEventDays", attendanceEventRetentionDays)
    .WaitForCompletion(migrationService);

var gateway = builder
    .AddContainer("gateway", "caddy", "2-alpine")
    .WithHttpEndpoint(port: 80, targetPort: 80, name: "http")
    .WithContainerRegistry(registry)
    .WithReference(api)
    .WaitFor(api);

if (builder.ExecutionContext.IsRunMode)
{
    var webapp = builder
        .AddViteApp("webapp", "../../../../webapp/Attendify")
        .WithEnvironment("VITE_PRIVACY_POLICY_CONTROLLER_NAME", controllerName)
        .WithEnvironment("VITE_PRIVACY_POLICY_CONTROLLER_ADDRESS", controllerAddress)
        .WithEnvironment("VITE_PRIVACY_POLICY_CONTROLLER_EMAIL", controllerEmail)
        .WithEnvironment("VITE_PRIVACY_POLICY_DPO_CONTACT", dpoContact)
        .WithEnvironment("VITE_PRIVACY_POLICY_AUTHORITY_NAME", authorityName)
        .WithEnvironment("VITE_PRIVACY_POLICY_AUTHORITY_URL", authorityUrl)
        .WithEnvironment("VITE_PRIVACY_POLICY_LEGAL_BASIS_DETAILS", legalBasisDetails)
        .WithEnvironment(
            "VITE_PRIVACY_POLICY_BIOMETRIC_CONDITION_DETAILS",
            biometricConditionDetails
        )
        .WithEnvironment("VITE_PRIVACY_POLICY_RETENTION_DETAILS", retentionDetails)
        .WithEnvironment("VITE_PRIVACY_POLICY_RECIPIENTS_DETAILS", recipientsDetails)
        .WithEnvironment("VITE_PRIVACY_POLICY_TRANSFER_DETAILS", transferDetails)
        .WithEnvironment("VITE_PRIVACY_POLICY_DPIA_DETAILS", dpiaDetails)
        .WithReference(api)
        .WaitFor(api)
        .WithExternalHttpEndpoints();

    gateway
        .WithReference(webapp)
        .WaitFor(webapp)
        .WithContainerFiles(
            "/etc/caddy",
            [
                new ContainerFile
                {
                    Name = "Caddyfile",
                    Contents = """
                    :80 {
                        handle /api/* {
                            reverse_proxy {$services__api__http__0}
                        }

                        handle {
                            reverse_proxy {$services__webapp__http__0} {
                                header_up Host aspire.dev.internal
                            }
                        }
                    }
                    """,
                },
            ]
        );
}
else
{
    gateway
        .WithDockerfile("../../../../", "backend/Attendify/tools/AppHost/Dockerfile.gateway")
        .WithBuildArg("VITE_PRIVACY_POLICY_CONTROLLER_NAME", controllerName)
        .WithBuildArg("VITE_PRIVACY_POLICY_CONTROLLER_ADDRESS", controllerAddress)
        .WithBuildArg("VITE_PRIVACY_POLICY_CONTROLLER_EMAIL", controllerEmail)
        .WithBuildArg("VITE_PRIVACY_POLICY_DPO_CONTACT", dpoContact)
        .WithBuildArg("VITE_PRIVACY_POLICY_AUTHORITY_NAME", authorityName)
        .WithBuildArg("VITE_PRIVACY_POLICY_AUTHORITY_URL", authorityUrl);
}

gateway.WithCloudflareTunnel(tunnel, hostname: hostname);

builder.Build().Run();