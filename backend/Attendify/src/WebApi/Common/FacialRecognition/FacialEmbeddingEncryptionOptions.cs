using System.ComponentModel.DataAnnotations;

namespace Attendify.Common.FacialRecognition;

public sealed class FacialEmbeddingEncryptionOptions
{
    public const string SectionName = "FacialEmbedding";

    [Required]
    [Base64String]
    public string EncryptionKey { get; init; } = null!;
}
