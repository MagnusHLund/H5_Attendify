using System.ComponentModel.DataAnnotations;

namespace Attendify.Common.FacialRecognition;

public sealed class FacialEmbeddingEncryptionOptions
{
    public const string SectionName = "FacialEmbedding";

    [Required]
    public string EncryptionKey { get; init; } = null!;
}
