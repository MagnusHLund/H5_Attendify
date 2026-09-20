using Attendify.Common.Domain.Users;
using Attendify.Common.Domain.FacialRecognition;
using Attendify.Common.FacialRecognition;
using Attendify.Common.Services;
using Microsoft.AspNetCore.Identity;

namespace Attendify.Features.Auth.RegisterStudent;

public sealed class RegisterStudentEndpoint(
    ApplicationDbContext dbContext,
    IPasswordHasher<User> passwordHasher,
    IStudentIdProtector studentIdProtector,
    IFacialEmbeddingService facialEmbeddingService,
    IEmbeddingEncryptor embeddingEncryptor
) : Endpoint<RegisterStudentRequest, RegisterStudentResponse>
{
    private const long MaxPhotoSizeBytes = 5 * 1024 * 1024;

    public override void Configure()
    {
        Post("/register");
        Group<AuthenticationGroup>();
        AllowFormData(true);
        AllowFileUploads(true);
        Description(x => x.WithName("RegisterStudent"));
    }

    public override async Task HandleAsync(RegisterStudentRequest request, CancellationToken ct)
    {
        string email = request.Email.Trim().ToLowerInvariant();

        if (await dbContext.Users.AnyAsync(user => user.Email == email, ct))
        {
            AddError(request => request.Email, "An account with this email already exists.");
            await Send.ErrorsAsync(StatusCodes.Status409Conflict, ct);
            return;
        }

        if (!await dbContext.EducationalInstitutes.AnyAsync(
                institute => institute.Id == request.EducationalInstituteId,
                ct
            ))
        {
            AddError(
                request => request.EducationalInstituteId,
                "The selected educational institute does not exist."
            );
            await Send.ErrorsAsync(StatusCodes.Status400BadRequest, ct);
            return;
        }

        IFormFile[] photos = [request.StraightPhoto, request.LeftPhoto, request.RightPhoto];

        if (photos.Any(photo => photo.Length is 0 or > MaxPhotoSizeBytes ||
                                !photo.ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase)))
        {
            AddError("Each face photo must be an image no larger than 5 MB.");
            await Send.ErrorsAsync(StatusCodes.Status400BadRequest, ct);
            return;
        }

        IReadOnlyList<byte[]> embeddings;

        try
        {
            embeddings = await facialEmbeddingService.CreateEmbeddingsAsync(photos, ct);
        }
        catch (FacePhotoValidationException exception)
        {
            AddError(exception.Message);
            await Send.ErrorsAsync(StatusCodes.Status400BadRequest, ct);
            return;
        }

        string encryptedStudentId = studentIdProtector.Protect(request.StudentId);
        var user = Attendify.Common.Domain.Users.User.Create(
            request.EducationalInstituteId,
            email,
            passwordHasher.HashPassword(null!, request.Password),
            encryptedStudentId
        );
        FacialProfile facialProfile = FacialProfile.Create(user);
        user.FacialProfile = facialProfile;

        foreach (byte[] embedding in embeddings)
        {
            EncryptedEmbedding encryptedEmbedding = embeddingEncryptor.Encrypt(embedding);
            facialProfile.FacialEmbeddings.Add(
                FacialEmbedding.Create(encryptedEmbedding.Ciphertext, encryptedEmbedding.Nonce)
            );
        }

        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync(ct);

        await Send.ResponseAsync(
            new RegisterStudentResponse(user.Id),
            StatusCodes.Status201Created,
            ct
        );
    }
}
