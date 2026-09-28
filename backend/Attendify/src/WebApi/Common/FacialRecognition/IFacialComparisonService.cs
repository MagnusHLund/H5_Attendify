namespace Attendify.Common.FacialRecognition;

public interface IFacialComparisonService
{
    bool IsMatch(IReadOnlyList<byte[]> referenceEmbeddings, byte[] candidateEmbedding);
}