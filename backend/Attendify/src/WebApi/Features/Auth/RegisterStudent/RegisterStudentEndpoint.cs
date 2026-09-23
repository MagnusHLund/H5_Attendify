using Attendify.Common.Authentication;
using Attendify.Common.Domain.FacialRecognition;
using Attendify.Common.Domain.Users;
using Attendify.Common.Encoding;
using Attendify.Common.FacialRecognition;
using Attendify.Common.Services;
using Attendify.Features.Auth.Shared;
using Microsoft.AspNetCore.Identity;
using Serilog;

namespace Attendify.Features.Auth.RegisterStudent;

public sealed class RegisterStudentEndpoint(
    ApplicationDbContext dbContext,
    IPasswordHasher<User> passwordHasher,
    IStudentIdProtector studentIdProtector,
    IFacialEmbeddingService facialEmbeddingService,
    IEmbeddingEncryptor embeddingEncryptor,
    IAuthenticationSessionService authenticationSessionService,
    IServiceScopeFactory serviceScopeFactory
) : Endpoint<RegisterStudentRequest>
{
    private const long MaxPhotoSizeBytes = 5 * 1024 * 1024;

    private readonly ILogger _logger = Log.ForContext<RegisterStudentEndpoint>();

    public override void Configure()
    {
        Post("/register");
        Group<AuthenticationGroup>();
        AllowAnonymous();
        Description(x => x.WithName("RegisterStudent"));
    }

    public override async Task HandleAsync(RegisterStudentRequest request, CancellationToken ct)
    {
        string email = EmailNormalizer.Normalize(request.Email);

        byte[][] photos;

        try
        {
            photos = GetPhotos(request);
        }
        catch (ArgumentException)
        {
            await SendPhotoValidationError(ct);
            return;
        }

        _logger.Information(
            "Registering student for educational institute {EducationalInstituteId} with photo sizes {StraightPhotoSize}, {LeftPhotoSize}, and {RightPhotoSize}",
            request.EducationalInstituteId,
            photos[0].Length,
            photos[1].Length,
            photos[2].Length
        );

        if (await EmailAlreadyExists(email, ct))
        {
            _logger.Warning(
                "Student registration rejected because the email is already registered for educational institute {EducationalInstituteId}",
                request.EducationalInstituteId
            );

            await SendEmailAlreadyExistsError(request, ct);
            return;
        }

        if (!await EducationalInstituteExists(request.EducationalInstituteId, ct))
        {
            _logger.Warning(
                "Student registration rejected because educational institute {EducationalInstituteId} does not exist",
                request.EducationalInstituteId
            );

            await SendEducationalInstituteNotFoundError(request, ct);
            return;
        }

        if (!ValidatePhotos(photos))
        {
            _logger.Warning(
                "Student registration rejected because one or more face photos are invalid for educational institute {EducationalInstituteId}",
                request.EducationalInstituteId
            );

            await SendPhotoValidationError(ct);
            return;
        }

        IReadOnlyList<byte[]> embeddings;

        try
        {
            embeddings = await facialEmbeddingService.CreateEmbeddingsAsync(photos, ct);
        }
        catch (FacePhotoValidationException exception)
        {
            _logger.Warning(
                "Student registration rejected because face photos could not be processed for educational institute {EducationalInstituteId}: {Reason}",
                request.EducationalInstituteId,
                exception.Message
            );

            AddError(exception.Message);
            await Send.ErrorsAsync(StatusCodes.Status400BadRequest, ct);

            return;
        }

        var strategy = dbContext.Database.CreateExecutionStrategy();
        AuthenticationSession? session = null;
        int registeredUserId = 0;

        await strategy.ExecuteAsync(async () =>
        {
            await using AsyncServiceScope scope = serviceScopeFactory.CreateAsyncScope();
            ApplicationDbContext attemptDbContext =
                scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            IAuthenticationSessionService attemptSessionService =
                scope.ServiceProvider.GetRequiredService<IAuthenticationSessionService>();
            await using var transaction = await attemptDbContext.Database.BeginTransactionAsync(ct);

            try
            {
                User? user = await attemptDbContext.Users.SingleOrDefaultAsync(
                    candidate => candidate.Email == email,
                    ct
                );

                if (user is null)
                {
                    user = CreateUser(request, email, embeddings);
                    attemptDbContext.Users.Add(user);
                    await attemptDbContext.SaveChangesAsync(ct);
                }

                if (session is null || !await attemptSessionService.IsPersistedAsync(user, session, ct))
                {
                    session = await attemptSessionService.CreateSessionAsync(user, ct);
                }

                registeredUserId = user.Id;

                await transaction.CommitAsync(ct);
            }
            catch
            {
                await transaction.RollbackAsync(ct);
                throw;
            }
        });

        authenticationSessionService.SetSessionCookies(
            session ?? throw new InvalidOperationException("Registration session was not created.")
        );

        _logger.Information(
            "Student registration completed for user {UserId} at educational institute {EducationalInstituteId}",
            registeredUserId,
            request.EducationalInstituteId
        );

        await Send.CreatedAtAsync<RegisterStudentEndpoint>(cancellation: ct);
    }

    private async Task<bool> EmailAlreadyExists(string email, CancellationToken ct)
    {
        return await dbContext.Users.AnyAsync(user => user.Email == email, ct);
    }

    private async Task<bool> EducationalInstituteExists(Guid instituteId, CancellationToken ct)
    {
        return await dbContext.EducationalInstitutes.AnyAsync(
            institute => institute.Id == instituteId,
            ct
        );
    }

    private static byte[][] GetPhotos(RegisterStudentRequest request)
    {
        return
        [
            Base64Encoding.Decode(request.StraightPhoto),
            Base64Encoding.Decode(request.LeftPhoto),
            Base64Encoding.Decode(request.RightPhoto),
        ];
    }

    private static bool ValidatePhotos(IReadOnlyCollection<byte[]> photos)
    {
        return photos.All(IsValidPhoto);
    }

    private static bool IsValidPhoto(byte[] photo)
    {
        return photo.Length is > 0 and <= (int)MaxPhotoSizeBytes;
    }

    private User CreateUser(
        RegisterStudentRequest request,
        string email,
        IReadOnlyList<byte[]> embeddings
    )
    {
        string encryptedStudentId = studentIdProtector.Protect(request.StudentId);

        User user = Common.Domain.Users.User.Create(
            request.EducationalInstituteId,
            email,
            passwordHasher.HashPassword(null!, request.Password),
            encryptedStudentId
        );

        FacialProfile facialProfile = FacialProfile.Create(user);
        user.FacialProfile = facialProfile;

        AddEmbeddings(facialProfile, embeddings);

        return user;
    }

    private void AddEmbeddings(FacialProfile facialProfile, IReadOnlyList<byte[]> embeddings)
    {
        foreach (byte[] embedding in embeddings)
        {
            EncryptedEmbedding encryptedEmbedding = embeddingEncryptor.Encrypt(embedding);

            facialProfile.FacialEmbeddings.Add(
                FacialEmbedding.Create(encryptedEmbedding.Ciphertext, encryptedEmbedding.Nonce)
            );
        }
    }

    private async Task SendEmailAlreadyExistsError(
        RegisterStudentRequest request,
        CancellationToken ct
    )
    {
        AddError(request => request.Email, "An account with this email already exists.");

        await Send.ErrorsAsync(StatusCodes.Status409Conflict, ct);
    }

    private async Task SendEducationalInstituteNotFoundError(
        RegisterStudentRequest request,
        CancellationToken ct
    )
    {
        AddError(
            request => request.EducationalInstituteId,
            "The selected educational institute does not exist."
        );

        await Send.ErrorsAsync(StatusCodes.Status400BadRequest, ct);
    }

    private async Task SendPhotoValidationError(CancellationToken ct)
    {
        AddError("Each face photo must be a valid image no larger than 5 MB.");

        await Send.ErrorsAsync(StatusCodes.Status400BadRequest, ct);
    }
}