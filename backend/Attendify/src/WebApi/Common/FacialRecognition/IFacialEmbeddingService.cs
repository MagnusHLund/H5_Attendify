using Microsoft.AspNetCore.Http;

namespace Attendify.Common.FacialRecognition;

public interface IFacialEmbeddingService
{
    Task<IReadOnlyList<byte[]>> CreateEmbeddingsAsync(
        IReadOnlyList<IFormFile> photos,
        CancellationToken cancellationToken
    );
}
