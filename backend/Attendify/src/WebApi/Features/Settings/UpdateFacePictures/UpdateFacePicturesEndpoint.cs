using Attendify.Common.Authentication;
using Attendify.Common.Domain.FacialRecognition;
using Attendify.Common.Encoding;
using Attendify.Common.FacialRecognition;
using Attendify.Common.Interfaces;

namespace Attendify.Features.Settings.UpdateFacePictures;

public sealed class UpdateFacePicturesEndpoint(
    ApplicationDbContext dbContext,
    ICurrentUserService currentUserService,
    IFacialEmbeddingService facialEmbeddingService,
    IEmbeddingEncryptor embeddingEncryptor,
    IServiceScopeFactory serviceScopeFactory
) : Endpoint<UpdateFacePicturesRequest>
{
    private const int MaxPhotoSizeBytes = 5 * 1024 * 1024;

    public override void Configure()
    {
        Patch("/face-photos");
        Group<SettingsGroup>();
        Policies(JwtOptions.AuthenticatedUserPolicy);
        Description(x => x.WithName("UpdateFacePictures"));
    }

    public override async Task HandleAsync(UpdateFacePicturesRequest request, CancellationToken ct)
    {
        if (currentUserService.UserType != UserType.Student)
        {
            await Send.ForbiddenAsync(ct);
            return;
        }

        if (!int.TryParse(currentUserService.UserId, out int userId))
        {
            await Send.UnauthorizedAsync(ct);
            return;
        }

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

        if (photos.Any(photo => photo.Length is 0 or > MaxPhotoSizeBytes))
        {
            await SendPhotoValidationError(ct);
            return;
        }

        if (!await dbContext.Users.AnyAsync(user => user.Id == userId, ct))
        {
            await Send.NotFoundAsync(ct);
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

        EncryptedEmbedding[] encryptedEmbeddings = embeddings
            .Select(embedding => embeddingEncryptor.Encrypt(embedding))
            .ToArray();

        var strategy = dbContext.Database.CreateExecutionStrategy();

        bool updated = await strategy.ExecuteAsync(async () =>
        {
            await using AsyncServiceScope scope = serviceScopeFactory.CreateAsyncScope();
            ApplicationDbContext attemptDbContext =
                scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            await using var transaction = await attemptDbContext.Database.BeginTransactionAsync(ct);

            FacialProfile? profile = await attemptDbContext
                .FacialProfiles.Include(candidate => candidate.FacialEmbeddings)
                .SingleOrDefaultAsync(candidate => candidate.UserId == userId, ct);

            if (profile is null)
            {
                return false;
            }

            attemptDbContext.FacialEmbeddings.RemoveRange(profile.FacialEmbeddings);
            profile.FacialEmbeddings.Clear();

            foreach (EncryptedEmbedding embedding in encryptedEmbeddings)
            {
                profile.FacialEmbeddings.Add(
                    FacialEmbedding.Create(embedding.Ciphertext, embedding.Nonce)
                );
            }

            await attemptDbContext.SaveChangesAsync(ct);
            await transaction.CommitAsync(ct);

            return true;
        });

        if (!updated)
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        await Send.NoContentAsync(ct);
    }

    private static byte[][] GetPhotos(UpdateFacePicturesRequest request)
    {
        return new[]
        {
            Base64Encoding.Decode(request.StraightPhoto),
            Base64Encoding.Decode(request.LeftPhoto),
            Base64Encoding.Decode(request.RightPhoto),
        };
    }

    private async Task SendPhotoValidationError(CancellationToken ct)
    {
        AddError("Each face photo must be valid Base64 for an image no larger than 5 MB.");
        await Send.ErrorsAsync(StatusCodes.Status400BadRequest, ct);
    }
}
