using Attendify.Common.Domain.Users;
using Attendify.Common.FacialRecognition;

namespace Attendify.Features.Attendance.CreateAttendance;

public sealed class FacialUserIdentifier(
    ApplicationDbContext dbContext,
    IFacialEmbeddingService embeddingService,
    IFacialComparisonService comparisonService,
    IEmbeddingEncryptor embeddingEncryptor
) : IFacialUserIdentifier
{
    public async Task<UserId> IdentifyUserAsync(
        IFormFile image,
        CancellationToken cancellationToken
    )
    {
        byte[] imageBytes;

        try
        {
            using var memoryStream = new MemoryStream();
            await image.OpenReadStream().CopyToAsync(memoryStream, cancellationToken);
            imageBytes = memoryStream.ToArray();
        }
        catch (FormatException)
        {
            throw new ArgumentException("The provided image is not valid base64.", nameof(image));
        }

        byte[] candidateEmbedding = await embeddingService.CreateEmbeddingAsync(
            imageBytes,
            cancellationToken
        );

        var users = await dbContext
            .Users.AsNoTracking()
            .Where(user =>
                user.AttendanceEnabled
                && !user.IsDeleted
                && user.FacialProfile != null
                && user.FacialProfile.FacialEmbeddings.Any()
            )
            .Select(user => new
            {
                user.Id,
                Embeddings = user.FacialProfile!.FacialEmbeddings.Select(embedding => new
                    {
                        embedding.EncryptedEmbedding,
                        embedding.Nonce,
                    })
                    .ToList(),
            })
            .ToListAsync(cancellationToken);

        foreach (var user in users)
        {
            List<byte[]> decryptedEmbeddings = user
                .Embeddings.Select(embedding =>
                    embeddingEncryptor
                        .Decrypt(embedding.EncryptedEmbedding, embedding.Nonce)
                        .Plaintext
                )
                .ToList();
            if (comparisonService.IsMatch(decryptedEmbeddings, candidateEmbedding))
            {
                return (UserId)user.Id;
            }
        }

        throw new NoMatchingUserException("No matching user was found.");
    }
}
