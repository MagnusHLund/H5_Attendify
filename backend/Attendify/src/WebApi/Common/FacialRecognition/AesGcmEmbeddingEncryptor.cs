using System.Security.Cryptography;
using Microsoft.Extensions.Options;

namespace Attendify.Common.FacialRecognition;

public sealed class AesGcmEmbeddingEncryptor : IEmbeddingEncryptor
{
    private const int KeyLength = 32;
    private const int NonceLength = 12;
    private const int TagLength = 16;
    private readonly byte[] _key;

    public AesGcmEmbeddingEncryptor(IOptions<FacialEmbeddingEncryptionOptions> options)
    {
        _key = Convert.FromBase64String(options.Value.EncryptionKey);

        if (_key.Length != KeyLength)
        {
            throw new InvalidOperationException(
                "The facial embedding encryption key must be 32 bytes."
            );
        }
    }

    public EncryptedEmbedding Encrypt(byte[] embedding)
    {
        ThrowIfNull(embedding);

        byte[] nonce = RandomNumberGenerator.GetBytes(NonceLength);
        byte[] ciphertextWithTag = new byte[embedding.Length + TagLength];

        using var aes = new AesGcm(_key, TagLength);
        aes.Encrypt(
            nonce,
            embedding,
            ciphertextWithTag.AsSpan(0, embedding.Length),
            ciphertextWithTag.AsSpan(embedding.Length, TagLength)
        );

        return new EncryptedEmbedding(ciphertextWithTag, nonce);
    }

    public DecryptedEmbedding Decrypt(byte[] ciphertextWithTag, byte[] nonce)
    {
        ThrowIfNull(ciphertextWithTag);
        ThrowIfNull(nonce);

        if (nonce.Length != NonceLength)
        {
            throw new ArgumentException(
                "Invalid nonce length.",
                nameof(nonce));
        }

        if (ciphertextWithTag.Length < TagLength)
        {
            throw new ArgumentException(
                "Invalid ciphertext.",
                nameof(ciphertextWithTag));
        }

        int ciphertextLength = ciphertextWithTag.Length - TagLength;

        byte[] plaintext = new byte[ciphertextLength];

        using var aes = new AesGcm(_key, TagLength);

        aes.Decrypt(
            nonce,
            ciphertextWithTag.AsSpan(0, ciphertextLength),
            ciphertextWithTag.AsSpan(ciphertextLength, TagLength),
            plaintext
        );

        return new DecryptedEmbedding(plaintext);
    }

}