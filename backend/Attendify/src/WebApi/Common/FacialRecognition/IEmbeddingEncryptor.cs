namespace Attendify.Common.FacialRecognition;

public interface IEmbeddingEncryptor
{
    EncryptedEmbedding Encrypt(byte[] embedding);
}

public sealed record EncryptedEmbedding(byte[] Ciphertext, byte[] Nonce);
