using System.Text;
using Attendify.Common.Authentication;
using Attendify.Common.Domain.Users;
using Attendify.Common.Email;
using Attendify.Common.FacialRecognition;
using Attendify.Common.Interfaces;
using Attendify.Common.Services;
using Attendify.Features.Attendance.CreateAttendance;
using Attendify.Features.Auth.PasswordReset;
using Attendify.Features.Auth.RequestPasswordReset;
using FastEndpoints.Security;
using FastEndpoints.Swagger;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Resend;
using System.Threading.RateLimiting;

namespace Attendify.Host;

public static class DependencyInjection
{
    public static void AddWebApi(this IHostApplicationBuilder builder)
    {
        var services = builder.Services;

        services.AddHttpContextAccessor();

        string resendApiKey =
            builder.Configuration.GetRequiredSection("Resend")["ApiKey"]
            ?? throw new InvalidOperationException("Resend:ApiKey is required.");

        services.AddResend(options => options.ApiToken = resendApiKey);

        IConfigurationSection emailSection = builder.Configuration.GetRequiredSection(
            EmailOptions.SectionName
        );

        services
            .AddOptions<EmailOptions>()
            .Bind(emailSection)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddScoped<IEmailSender, ResendEmailSender>();

        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            options.AddPolicy(
                PasswordResetRequestRateLimit.PolicyName,
                context =>
                    RateLimitPartition.GetFixedWindowLimiter(
                        partitionKey: context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                        factory: _ => new FixedWindowRateLimiterOptions
                        {
                            PermitLimit = PasswordResetRequestRateLimit.PermitLimit,
                            Window = PasswordResetRequestRateLimit.Window,
                            QueueLimit = 0,
                            AutoReplenishment = true,
                        }
                    )
            );
        });

        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddDataProtection();
        services.AddSingleton<IStudentIdProtector, StudentIdProtector>();
        services.AddSingleton<IPasswordHasher<User>, PasswordHasher<User>>();
        services.AddSingleton<IFacialEmbeddingService, FaceAiSharpEmbeddingService>();
        services.AddSingleton<IFacialComparisonService, FaceAiSharpComparisonService>();
        services.AddScoped<IFacialUserIdentifier, FacialUserIdentifier>();
        services.AddSingleton<IEmbeddingEncryptor, AesGcmEmbeddingEncryptor>();

        IConfigurationSection facialEmbeddingSection = builder.Configuration.GetRequiredSection(
            FacialEmbeddingEncryptionOptions.SectionName
        );

        services.AddSingleton<
            IValidateOptions<FacialEmbeddingEncryptionOptions>,
            FacialEmbeddingEncryptionOptionsValidator
        >();

        services
            .AddOptions<FacialEmbeddingEncryptionOptions>()
            .Bind(facialEmbeddingSection)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        IConfigurationSection jwtSection = builder.Configuration.GetRequiredSection(
            JwtOptions.SectionName
        );

        IConfigurationSection refreshTokenSection = builder.Configuration.GetRequiredSection(
            RefreshTokenOptions.SectionName
        );

        IConfigurationSection resetPasswordTokenSection = builder.Configuration.GetRequiredSection(
            ResetPasswordTokenOptions.SectionName
        );

        JwtOptions jwtOptions =
            jwtSection.Get<JwtOptions>()
            ?? throw new InvalidOperationException("JWT configuration is required.");

        RefreshTokenOptions refreshTokenOptions =
            refreshTokenSection.Get<RefreshTokenOptions>()
            ?? throw new InvalidOperationException("Refresh token configuration is required.");

        services.AddScoped<IAuthenticationSessionService, AuthenticationSessionService>();
        services.AddScoped<IAuthenticationCookieService, AuthenticationCookieService>();
        services.AddScoped<IRefreshTokenService, RefreshTokenService>();
        services.AddScoped<IJwtTokenService, JwtTokenService>();

        services
            .AddOptions<ResetPasswordTokenOptions>()
            .Bind(resetPasswordTokenSection)
            .ValidateDataAnnotations()
            .Validate(
                options => options.HasValidSecurityCodeHashKey(),
                "ResetPasswordToken:SecurityCodeHashKey must be Base64 for exactly 32 bytes."
            )
            .ValidateOnStart();

        services
            .AddOptions<JwtOptions>()
            .Bind(jwtSection)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services
            .AddOptions<RefreshTokenOptions>()
            .Bind(refreshTokenSection)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.Configure<JwtCreationOptions>(options =>
        {
            options.SigningKey = jwtOptions.SigningKey;
            options.Issuer = jwtOptions.Issuer;
            options.Audience = jwtOptions.Audience;
        });

        services.AddAuthenticationJwtBearer(
            options => options.SigningKey = jwtOptions.SigningKey,
            options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = jwtOptions.Issuer,
                    ValidateAudience = true,
                    ValidAudience = jwtOptions.Audience,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(jwtOptions.SigningKey)
                    ),
                    ClockSkew = TimeSpan.Zero,
                };

                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        context.Token = context.Request.Cookies["AccessToken"];

                        return Task.CompletedTask;
                    },
                };
            }
        );

        services.AddAuthorization(options =>
        {
            options.AddPolicy(
                JwtOptions.AuthenticatedUserPolicy,
                policy => policy.RequireAuthenticatedUser()
            );
        });

        services.AddOpenApi();

        services.AddFastEndpoints();

        builder.Services.SwaggerDocument();

        builder.Services.ConfigureHttpJsonOptions(options =>
        {
            options.SerializerOptions.Converters.Add(
                new System.Text.Json.Serialization.JsonStringEnumConverter()
            );
        });
    }

    public static void AddApplication(this IHostApplicationBuilder builder)
    {
        var applicationAssembly = typeof(DependencyInjection).Assembly;
        var services = builder.Services;

        services.AddValidatorsFromAssembly(applicationAssembly, includeInternalTypes: true);
        services.AddScoped<IPasswordResetService, PasswordResetService>();
        services.AddSingleton<PasswordResetRequestQueue>();
        services.AddSingleton<IPasswordResetRequestQueue>(serviceProvider =>
            serviceProvider.GetRequiredService<PasswordResetRequestQueue>()
        );
        services.AddHostedService<PasswordResetRequestBackgroundService>();
    }
}