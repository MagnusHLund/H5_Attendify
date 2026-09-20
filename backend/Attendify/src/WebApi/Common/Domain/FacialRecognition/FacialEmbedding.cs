using Attendify.Common.Domain.Base;

namespace Attendify.Common.Domain.FacialRecognition;

public sealed class FacialEmbedding : AggregateRoot<int>
{
    public int FacialProfileId { get; set; }

    public byte[] EncryptedEmbedding { get; set; } = null!;

    public byte[] Nonce { get; set; } = null!;

    public FacialProfile FacialProfile { get; set; } = null!;

    private FacialEmbedding() { }
}
