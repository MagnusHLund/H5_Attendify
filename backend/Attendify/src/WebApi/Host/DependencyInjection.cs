using System.Text;
using Attendify.Common.Authentication;
using Attendify.Common.Domain.Users;
using Attendify.Common.FacialRecognition;
using Attendify.Common.Interfaces;
using Attendify.Common.Services;
using FastEndpoints.Security;
using FastEndpoints.Swagger;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;

namespace Attendify.Host;

public static class DependencyInjection
{
    public static void AddWebApi(this IHostApplicationBuilder builder)
    {
        var services = builder.Services;

        services.AddHttpContextAccessor();

        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddDataProtection();
        services.AddSingleton<IStudentIdProtector, StudentIdProtector>();
        services.AddSingleton<IPasswordHasher<User>, PasswordHasher<User>>();
        services.AddSingleton<IFacialEmbeddingService, FaceAiSharpEmbeddingService>();
        services.AddSingleton<IEmbeddingEncryptor, AesGcmEmbeddingEncryptor>();

        IConfigurationSection facialEmbeddingSection = builder.Configuration.GetRequiredSection(
            FacialEmbeddingEncryptionOptions.SectionName
        );
        services
            .AddOptions<FacialEmbeddingEncryptionOptions>()
            .Bind(facialEmbeddingSection)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        IConfigurationSection jwtSection = builder.Configuration.GetRequiredSection(
            JwtOptions.SectionName
        );

        JwtOptions jwtOptions =
            jwtSection.Get<JwtOptions>()
            ?? throw new InvalidOperationException("JWT configuration is required.");

        services
            .AddOptions<JwtOptions>()
            .Bind(jwtSection)
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
    }
}
