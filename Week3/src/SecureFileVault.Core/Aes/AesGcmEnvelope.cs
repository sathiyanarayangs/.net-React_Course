using System.Security.Cryptography;
using System.Text;
using SecureFileVault.Core.Kdf;

namespace SecureFileVault.Core.Aes;

/// <summary>
/// Tasks 3.8-3.9 - password -> PBKDF2 -> AES-GCM, for data small enough to
/// hold in memory (small files, strings, tokens). For the 100 MB+ streaming
/// case see Streaming/GcmStreamVault.cs, which chunks this same idea over a
/// file instead of a single buffer.
///
/// Wire format (all bytes concatenated, in order):
///   salt (16) || nonce (12) || tag (16) || ciphertext (N)
///
/// Salt and nonce are NOT secret and travel with the ciphertext — the key
/// itself never does. The tag is what Task 3.9 proves matters: flip a
/// single bit anywhere in the ciphertext (or the tag) and AesGcm.Decrypt
/// throws AuthenticationTagMismatchException instead of silently handing
/// back tampered plaintext — the integrity gap Task 3.6 found in CBC.
/// </summary>
public static class AesGcmEnvelope
{
    private const int NonceSizeBytes = 12; // AES-GCM's standard nonce size
    private const int TagSizeBytes = 16;   // 128-bit authentication tag

    public static byte[] Encrypt(string password, byte[] plaintext)
    {
        byte[] salt = Pbkdf2KeyDerivation.GenerateSalt();
        byte[] key = Pbkdf2KeyDerivation.DeriveKey(password, salt);
        byte[] nonce = RandomNumberGenerator.GetBytes(NonceSizeBytes); // fresh every call — never reuse under one key

        byte[] ciphertext = new byte[plaintext.Length];
        byte[] tag = new byte[TagSizeBytes];

        using (var gcm = new AesGcm(key, TagSizeBytes))
        {
            gcm.Encrypt(nonce, plaintext, ciphertext, tag);
        }

        // Assemble the envelope: salt || nonce || tag || ciphertext
        byte[] envelope = new byte[salt.Length + nonce.Length + tag.Length + ciphertext.Length];
        int offset = 0;
        Buffer.BlockCopy(salt, 0, envelope, offset, salt.Length); offset += salt.Length;
        Buffer.BlockCopy(nonce, 0, envelope, offset, nonce.Length); offset += nonce.Length;
        Buffer.BlockCopy(tag, 0, envelope, offset, tag.Length); offset += tag.Length;
        Buffer.BlockCopy(ciphertext, 0, envelope, offset, ciphertext.Length);

        return envelope;
    }

    public static byte[] EncryptString(string password, string plaintext) =>
        Encrypt(password, Encoding.UTF8.GetBytes(plaintext));

    /// <summary>
    /// Decrypts an envelope produced by Encrypt(). Throws
    /// AuthenticationTagMismatchException (a CryptographicException) if the
    /// ciphertext or tag was tampered with, or if the password is wrong.
    /// </summary>
    public static byte[] Decrypt(string password, byte[] envelope)
    {
        if (envelope.Length < Pbkdf2KeyDerivation.SaltSizeBytes + NonceSizeBytes + TagSizeBytes)
            throw new ArgumentException("Envelope is too short to contain salt+nonce+tag.", nameof(envelope));

        int offset = 0;
        byte[] salt = envelope[offset..(offset += Pbkdf2KeyDerivation.SaltSizeBytes)];
        byte[] nonce = envelope[offset..(offset += NonceSizeBytes)];
        byte[] tag = envelope[offset..(offset += TagSizeBytes)];
        byte[] ciphertext = envelope[offset..];

        byte[] key = Pbkdf2KeyDerivation.DeriveKey(password, salt);
        byte[] plaintext = new byte[ciphertext.Length];

        using var gcm = new AesGcm(key, TagSizeBytes);
        gcm.Decrypt(nonce, ciphertext, tag, plaintext); // throws on tamper or wrong key
        return plaintext;
    }

    public static string DecryptToString(string password, byte[] envelope) =>
        Encoding.UTF8.GetString(Decrypt(password, envelope));
}
