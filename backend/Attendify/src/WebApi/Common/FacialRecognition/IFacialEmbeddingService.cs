namespace Attendify.Common.FacialRecognition;

public interface IFacialEmbeddingService
{
    Task<IReadOnlyList<byte[]>> CreateEmbeddingsAsync(
        IReadOnlyList<byte[]> photos,
        CancellationToken cancellationToken
    );

    Task<byte[]> CreateEmbeddingAsync(
     byte[] photo,
     CancellationToken cancellationToken);
}