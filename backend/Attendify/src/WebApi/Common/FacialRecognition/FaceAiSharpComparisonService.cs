namespace Attendify.Common.FacialRecognition;

public sealed class FaceAiSharpComparisonService : IFacialComparisonService
{
    private const float MatchThreshold = 0.5f;

    private readonly ILogger _logger = Log.ForContext<FaceAiSharpComparisonService>();


    public bool IsMatch(
    IReadOnlyList<byte[]> referenceEmbeddings,
    byte[] candidateEmbedding)
    {
        float[] candidate = BytesToFloats(candidateEmbedding);

        _logger.Debug(
            "Candidate embedding: {ByteLength} bytes, {Dimension} dimensions",
            candidateEmbedding.Length,
            candidate.Length);

        foreach (byte[] referenceBytes in referenceEmbeddings)
        {
            float[] reference = BytesToFloats(referenceBytes);

            _logger.Debug(
                "Reference embedding: {ByteLength} bytes, {Dimension} dimensions",
                referenceBytes.Length,
                reference.Length);

            if (reference.Length != candidate.Length)
            {
                _logger.Warning(
                    "Embedding dimension mismatch. Reference: {ReferenceDimension}, Candidate: {CandidateDimension}",
                    reference.Length,
                    candidate.Length);

                continue;
            }

            if (CosineSimilarity(reference, candidate) >= MatchThreshold)
            {
                return true;
            }
        }

        return false;
    }
    private static float[] BytesToFloats(byte[] bytes)
    {
        if (bytes.Length % sizeof(float) != 0)
        {
            throw new ArgumentException(
                "Embedding data is not a valid float array.",
                nameof(bytes));
        }

        float[] floats = new float[bytes.Length / sizeof(float)];

        Buffer.BlockCopy(
            bytes,
            0,
            floats,
            0,
            bytes.Length);

        return floats;
    }
    private static float CosineSimilarity(
    ReadOnlySpan<float> a,
    ReadOnlySpan<float> b)
    {
        float dot = 0;
        float magnitudeA = 0;
        float magnitudeB = 0;

        for (int i = 0; i < a.Length; i++)
        {
            dot += a[i] * b[i];
            magnitudeA += a[i] * a[i];
            magnitudeB += b[i] * b[i];
        }

        if (magnitudeA == 0 || magnitudeB == 0)
        {
            return 0;
        }

        return dot / (MathF.Sqrt(magnitudeA) * MathF.Sqrt(magnitudeB));
    }
}