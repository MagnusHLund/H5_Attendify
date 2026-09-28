namespace Attendify.Features.Attendance.CreateAttendance;

using Attendify.Common.FacialRecognition;
using Attendify.Common.Domain.Users;
using Attendify.Common.Domain.FacialRecognition;

public sealed class FacialUserIdentifier(
    ApplicationDbContext dbContext,
    IFacialEmbeddingService embeddingService,
    IFacialComparisonService comparisonService,
    IEmbeddingEncryptor embeddingEncryptor)
    : IFacialUserIdentifier
{
    public async Task<UserId> IdentifyUserAsync(
        string base64Image,
        CancellationToken cancellationToken)
    {
        byte[] imageBytes;

        try
        {
            imageBytes = Convert.FromBase64String(base64Image);
        }
        catch (FormatException)
        {
            throw new ArgumentException(
                "The provided image is not valid base64.",
                nameof(base64Image));
        }

        byte[] candidateEmbedding =
            await embeddingService.CreateEmbeddingAsync(
                imageBytes,
                cancellationToken);

        var users = await dbContext.Users
            .AsNoTracking()
            .Where(user =>
                user.FacialProfile != null &&
                user.FacialProfile.FacialEmbeddings.Any())
            .Select(user => new
            {
                user.Id,
                Embeddings = user.FacialProfile.FacialEmbeddings
                    .Select(embedding => new
                    {
                        embedding.EncryptedEmbedding,
                        embedding.Nonce
                    })
                    .ToList()
            })
            .ToListAsync(cancellationToken);

        foreach (var user in users)
        {
            List<byte[]> decryptedEmbeddings = user.Embeddings
                .Select(embedding =>
                    embeddingEncryptor.Decrypt(
                    embedding.EncryptedEmbedding,
                    embedding.Nonce).Plaintext)
                .ToList();
            if (comparisonService.IsMatch(
                decryptedEmbeddings,
                candidateEmbedding))
            {
                return (UserId)user.Id;
            }
        }

        throw new NoMatchingUserException(
            "No matching user was found.");
    }
}