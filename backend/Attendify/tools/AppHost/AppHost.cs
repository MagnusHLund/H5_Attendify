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
var legalBasisDetails = builder.Configuration["Parameters:PrivacyPolicyLegalBasisDetails"];
var biometricConditionDetails = builder.Configuration["Parameters:PrivacyPolicyBiometricConditionDetails"];
var retentionDetails = builder.Configuration["Parameters:PrivacyPolicyRetentionDetails"];
var recipientsDetails = builder.Configuration["Parameters:PrivacyPolicyRecipientsDetails"];
var transferDetails = builder.Configuration["Parameters:PrivacyPolicyTransferDetails"];
var dpiaDetails = builder.Configuration["Parameters:PrivacyPolicyDpiaDetails"];
var attendanceRecordRetentionDays = builder.Configuration["Parameters:AttendanceRecordRetentionDays"];
var attendanceDetectionRetentionDays = builder.Configuration["Parameters:AttendanceDetectionRetentionDays"];
var attendanceEventRetentionDays = builder.Configuration["Parameters:AttendanceEventRetentionDays"];

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
    .WaitForCompletion(migrationService);

if (attendanceRecordRetentionDays is not null)
    api.WithEnvironment("DataRetention__AttendanceRecordDays", attendanceRecordRetentionDays);
if (attendanceDetectionRetentionDays is not null)
    api.WithEnvironment("DataRetention__AttendanceDetectionDays", attendanceDetectionRetentionDays);
if (attendanceEventRetentionDays is not null)
    api.WithEnvironment("DataRetention__AttendanceEventDays", attendanceEventRetentionDays);

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

if (!string.IsNullOrWhiteSpace(legalBasisDetails))
    webapp.WithEnvironment("VITE_PRIVACY_POLICY_LEGAL_BASIS_DETAILS", legalBasisDetails);
if (!string.IsNullOrWhiteSpace(biometricConditionDetails))
    webapp.WithEnvironment("VITE_PRIVACY_POLICY_BIOMETRIC_CONDITION_DETAILS", biometricConditionDetails);
if (!string.IsNullOrWhiteSpace(retentionDetails))
    webapp.WithEnvironment("VITE_PRIVACY_POLICY_RETENTION_DETAILS", retentionDetails);
if (!string.IsNullOrWhiteSpace(recipientsDetails))
    webapp.WithEnvironment("VITE_PRIVACY_POLICY_RECIPIENTS_DETAILS", recipientsDetails);
if (!string.IsNullOrWhiteSpace(transferDetails))
    webapp.WithEnvironment("VITE_PRIVACY_POLICY_TRANSFER_DETAILS", transferDetails);
if (!string.IsNullOrWhiteSpace(dpiaDetails))
    webapp.WithEnvironment("VITE_PRIVACY_POLICY_DPIA_DETAILS", dpiaDetails);

var gateway = builder
    .AddYarp("gateway")
    .WithConfiguration(yarp =>
    {
        yarp.AddRoute("/api/{**catch-all}", api);
        yarp.AddRoute("{**catch-all}", webapp);
    });

gateway.WithCloudflareTunnel(tunnel, hostname: hostname);

builder.Build().Run();
