using FaceAiSharp;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Attendify.Common.FacialRecognition;

public sealed class FaceAiSharpEmbeddingService : IFacialEmbeddingService
{
    private const int RequiredPhotoCount = 3;

    private readonly IFaceDetectorWithLandmarks _faceDetector =
        FaceAiSharpBundleFactory.CreateFaceDetectorWithLandmarks();

    private readonly IFaceEmbeddingsGenerator _embeddingsGenerator =
        FaceAiSharpBundleFactory.CreateFaceEmbeddingsGenerator();

    public async Task<IReadOnlyList<byte[]>> CreateEmbeddingsAsync(
        IReadOnlyList<byte[]> photos,
        CancellationToken cancellationToken
    )
    {
        if (photos.Count != RequiredPhotoCount)
        {
            throw new FacePhotoValidationException("Exactly three face photos are required.");
        }

        var embeddings = new List<byte[]>(RequiredPhotoCount);

        foreach (byte[] photo in photos)
        {
            await using Stream stream = new MemoryStream(photo);

            Image<Rgb24> image;

            try
            {
                image = await Image.LoadAsync<Rgb24>(stream, cancellationToken);
            }
            catch (ImageFormatException)
            {
                throw new FacePhotoValidationException("Each face photo must be a valid image.");
            }

            using (image)
            {
                FaceDetectorResult[] faces = _faceDetector.DetectFaces(image).ToArray();

                if (faces.Length != 1 || faces[0].Landmarks is not IReadOnlyList<PointF> landmarks)
                {
                    throw new FacePhotoValidationException(
                        "Each photo must contain exactly one clearly visible face."
                    );
                }

                _embeddingsGenerator.AlignFaceUsingLandmarks(image, landmarks);

                float[] embedding = _embeddingsGenerator.GenerateEmbedding(image);

                byte[] embeddingBytes = new byte[embedding.Length * sizeof(float)];

                Buffer.BlockCopy(embedding, 0, embeddingBytes, 0, embeddingBytes.Length);

                embeddings.Add(embeddingBytes);
            }
        }

        return embeddings;
    }

    public async Task<byte[]> CreateEmbeddingAsync(
    byte[] photo,
    CancellationToken cancellationToken)
    {
        await using var stream = new MemoryStream(photo);

        Image<Rgb24> image;

        try
        {
            image = await Image.LoadAsync<Rgb24>(
                stream,
                cancellationToken);
        }
        catch (UnknownImageFormatException)
        {
            throw new FacePhotoValidationException(
                "The provided image must be a valid image.");
        }

        using (image)
        {
            FaceDetectorResult[] faces =
                _faceDetector.DetectFaces(image).ToArray();

            if (faces.Length != 1 ||
                faces[0].Landmarks is not IReadOnlyList<PointF> landmarks)
            {
                throw new FacePhotoValidationException(
                    "The image must contain exactly one clearly visible face.");
            }

            _embeddingsGenerator.AlignFaceUsingLandmarks(
                image,
                landmarks);

            float[] embedding =
                _embeddingsGenerator.GenerateEmbedding(image);

            byte[] embeddingBytes =
                new byte[embedding.Length * sizeof(float)];

            Buffer.BlockCopy(
                embedding,
                0,
                embeddingBytes,
                0,
                embeddingBytes.Length);

            return embeddingBytes;
        }
    }
}