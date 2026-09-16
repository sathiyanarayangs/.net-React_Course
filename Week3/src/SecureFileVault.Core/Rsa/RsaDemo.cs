using System.Security.Cryptography;
using System.Text;

namespace SecureFileVault.Core.Rsa;

/// <summary>
/// Task 3.13 (concept-level; the project does not use RSA anywhere else).
///
/// RSA is asymmetric: a PUBLIC key encrypts, the matching PRIVATE key
/// decrypts — the opposite pairing from a digital SIGNATURE, where the
/// PRIVATE key signs and the PUBLIC key verifies. Mixing these up is a
/// common conceptual error: "encrypt with private key" is not a thing RSA
/// encryption does (that's what signing is).
///
/// RSA has a hard SIZE LIMIT tied to the key size and padding scheme: a
/// 2048-bit key with OAEP-SHA256 padding can encrypt at most ~190 bytes in
/// one call. That's nowhere near enough for a file, or even a long
/// message — RSA is not a bulk cipher.
///
/// That's why real protocols (TLS included) use HYBRID encryption: RSA (or
/// another asymmetric algorithm) encrypts a random, one-time AES key —
/// that's small enough for RSA's limit — and AES (fast, no size limit)
/// encrypts the actual bulk data with that key. You get RSA's "no shared
/// secret needed in advance" property for exchanging the key, and AES's
/// speed for the data itself.
/// </summary>
public static class RsaDemo
{
    public static (byte[] PublicKey, byte[] PrivateKey) GenerateKeyPair()
    {
        using var rsa = System.Security.Cryptography.RSA.Create(2048);
        return (rsa.ExportRSAPublicKey(), rsa.ExportRSAPrivateKey());
    }

    /// <summary>Encrypts with the PUBLIC key. Only the matching private key can decrypt.</summary>
    public static byte[] EncryptShortMessage(byte[] publicKey, string message)
    {
        using var rsa = System.Security.Cryptography.RSA.Create();
        rsa.ImportRSAPublicKey(publicKey, out _);
        return rsa.Encrypt(Encoding.UTF8.GetBytes(message), RSAEncryptionPadding.OaepSHA256);
    }

    /// <summary>Decrypts with the PRIVATE key.</summary>
    public static string DecryptShortMessage(byte[] privateKey, byte[] ciphertext)
    {
        using var rsa = System.Security.Cryptography.RSA.Create();
        rsa.ImportRSAPrivateKey(privateKey, out _);
        byte[] plaintext = rsa.Decrypt(ciphertext, RSAEncryptionPadding.OaepSHA256);
        return Encoding.UTF8.GetString(plaintext);
    }

    /// <summary>Approximate max plaintext bytes for a given RSA key size with OAEP-SHA256 padding.</summary>
    public static int MaxPlaintextBytes(int keySizeBits, int hashOutputBytes = 32) =>
        (keySizeBits / 8) - (2 * hashOutputBytes) - 2;
}
