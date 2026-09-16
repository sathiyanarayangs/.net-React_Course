using System.Security.Cryptography;

namespace SecureFileVault.Core.Aes;

/// <summary>
/// Tasks 3.4-3.6 - symmetric AES-CBC through CryptoStream, plus the two
/// proof points the brief asks for: same plaintext -> different ciphertext
/// per fresh IV (3.5), and CBC's lack of an integrity signal (3.6).
/// </summary>
public static class AesCbcDemo
{
    /// <summary>
    /// Task 3.4 - 256-bit key, random IV, string round-trip through
    /// CryptoStream over a MemoryStream. Returns (iv, ciphertext); the IV
    /// is NOT secret and travels alongside the ciphertext (Task 3.5).
    /// </summary>
    public static (byte[] Iv, byte[] Ciphertext) Encrypt(byte[] key, string plaintext)
    {
        using var aes = System.Security.Cryptography.Aes.Create();
        aes.KeySize = 256;
        aes.Key = key;
        aes.GenerateIV(); // fresh, random IV every call — never reuse one under the same key

        using var output = new MemoryStream();
        using (var encryptor = aes.CreateEncryptor())
        using (var cryptoStream = new CryptoStream(output, encryptor, CryptoStreamMode.Write))
        using (var writer = new StreamWriter(cryptoStream))
        {
            writer.Write(plaintext);
        } // disposing the CryptoStream flushes the final block + padding

        return (aes.IV, output.ToArray());
    }

    public static string Decrypt(byte[] key, byte[] iv, byte[] ciphertext)
    {
        using var aes = System.Security.Cryptography.Aes.Create();
        aes.KeySize = 256;
        aes.Key = key;
        aes.IV = iv;

        using var input = new MemoryStream(ciphertext);
        using var decryptor = aes.CreateDecryptor();
        using var cryptoStream = new CryptoStream(input, decryptor, CryptoStreamMode.Read);
        using var reader = new StreamReader(cryptoStream);
        return reader.ReadToEnd();
        // A CryptographicException here (bad padding) is the ONLY signal
        // CBC gives you on tamper/wrong key — see DecryptWithWrongKeyThrows.
    }

    /// <summary>
    /// Task 3.6 - decrypting with the wrong key doesn't fail cleanly with
    /// "wrong key"; CBC has no integrity check at all, so what you get is a
    /// CryptographicException from PADDING happening to come out invalid
    /// (or, occasionally, garbage bytes that *coincidentally* pad correctly
    /// and decrypt to nonsense with NO exception at all). That's the
    /// integrity gap: CBC can't reliably tell you "this was tampered with,"
    /// only sometimes "the padding didn't line up." AES-GCM (Tasks 3.8-3.9)
    /// closes this gap with an authentication tag that is checked on every
    /// decrypt, tamper or not.
    /// </summary>
    public static bool DecryptWithWrongKeyThrows(byte[] correctKey, byte[] wrongKey, string plaintext)
    {
        var (iv, ciphertext) = Encrypt(correctKey, plaintext);
        try
        {
            Decrypt(wrongKey, iv, ciphertext);
            return false; // no exception: still not proof of tampering, just luck
        }
        catch (CryptographicException)
        {
            return true;
        }
    }
}
