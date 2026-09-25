namespace Attendify.Common.FacialRecognition;

public interface IEmbeddingEncryptor
{
    EncryptedEmbedding Encrypt(byte[] embedding);
    DecryptedEmbedding Decrypt(byte[] ciphertextWithTag, byte[] nonce);
}

public sealed record EncryptedEmbedding(byte[] Ciphertext, byte[] Nonce);
public sealed record DecryptedEmbedding(byte[] Plaintext);