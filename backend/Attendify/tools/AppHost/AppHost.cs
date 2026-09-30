using Projects;

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
    .WithEnvironment("Resend__ApiKey", resendApiKey)
    .WithEnvironment("Email__FromAddress", emailFromAddress)
    .WithEnvironment("Email__FromName", emailFromName)
    .WithEnvironment("ResetPasswordToken__SecurityCodeHashKey", passwordResetCodeHashKey)
    .WithEnvironment("Parameters__StudentAccessCodeGenSecret", studentAccessCodeSecret)
    .WithEnvironment("Jwt__SigningKey", jwtSigningKey)
    .WithEnvironment("FacialEmbedding__EncryptionKey", facialEmbeddingEncryptionKey)
    .WithEnvironment("DataRetention__AttendanceEventDays", attendanceEventRetentionDays)
    .WaitForCompletion(migrationService);

var webapp = builder
    .AddViteApp("webapp", "../../../../webapp/Attendify")
    .WithEnvironment("VITE_PRIVACY_POLICY_CONTROLLER_NAME", controllerName)
    .WithEnvironment("VITE_PRIVACY_POLICY_CONTROLLER_ADDRESS", controllerAddress)
    .WithEnvironment("VITE_PRIVACY_POLICY_CONTROLLER_EMAIL", controllerEmail)
    .WithEnvironment("VITE_PRIVACY_POLICY_DPO_CONTACT", dpoContact)
    .WithEnvironment("VITE_PRIVACY_POLICY_AUTHORITY_NAME", authorityName)
    .WithEnvironment("VITE_PRIVACY_POLICY_AUTHORITY_URL", authorityUrl)
    .WithEnvironment("VITE_PRIVACY_POLICY_LEGAL_BASIS_DETAILS", legalBasisDetails)
    .WithEnvironment("VITE_PRIVACY_POLICY_BIOMETRIC_CONDITION_DETAILS", biometricConditionDetails)
    .WithEnvironment("VITE_PRIVACY_POLICY_RETENTION_DETAILS", retentionDetails)
    .WithEnvironment("VITE_PRIVACY_POLICY_RECIPIENTS_DETAILS", recipientsDetails)
    .WithEnvironment("VITE_PRIVACY_POLICY_TRANSFER_DETAILS", transferDetails)
    .WithEnvironment("VITE_PRIVACY_POLICY_DPIA_DETAILS", dpiaDetails)
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
