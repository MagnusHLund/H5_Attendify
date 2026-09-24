using Microsoft.Extensions.Options;

namespace Attendify.Common.FacialRecognition;

public class FacialEmbeddingEncryptionOptionsValidator
    : IValidateOptions<FacialEmbeddingEncryptionOptions>
{
    public ValidateOptionsResult Validate(string? name, FacialEmbeddingEncryptionOptions options)
    {
        if (string.IsNullOrWhiteSpace(options.EncryptionKey))
        {
            return ValidateOptionsResult.Fail(
                $"{FacialEmbeddingEncryptionOptions.SectionName}:EncryptionKey is required."
            );
        }

        byte[] key;

        try
        {
            key = Convert.FromBase64String(options.EncryptionKey);
        }
        catch (FormatException)
        {
            return ValidateOptionsResult.Fail(
                $"{FacialEmbeddingEncryptionOptions.SectionName}:EncryptionKey must be a valid Base64 string."
            );
        }

        if (key.Length != 32)
        {
            return ValidateOptionsResult.Fail(
                $"{FacialEmbeddingEncryptionOptions.SectionName}:EncryptionKey must decode to exactly 32 bytes."
            );
        }

        return ValidateOptionsResult.Success;
    }
}
