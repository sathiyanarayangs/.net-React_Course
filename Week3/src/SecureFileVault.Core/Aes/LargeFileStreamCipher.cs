using System.Security.Cryptography;

namespace SecureFileVault.Core.Aes;

/// <summary>
/// Task 3.12 - encrypt/decrypt a file of any size (including 100 MB+)
/// without ever holding the whole thing in memory. FileStream is chained
/// directly into CryptoStream; .CopyTo moves data in its own internal
/// chunks, so at no point does the full plaintext or ciphertext exist as a
/// single in-memory byte[].
///
/// AES-CBC (not GCM) is used here specifically because CryptoStream's
/// block-cipher-mode design streams naturally with CBC; .NET's AesGcm type
/// works over complete buffers, not a CryptoStream-compatible interface —
/// that's exactly why the actual SecureFileVault tool (Streaming/GcmStreamVault.cs)
/// re-derives GCM's guarantees itself by chunking the file and running a
/// fresh AesGcm encrypt/decrypt per chunk, rather than trying to force GCM
/// through CryptoStream.
/// </summary>
public static class LargeFileStreamCipher
{
    /// <summary>CryptoStreamMode.Write: plaintext flows IN, ciphertext is what gets written to disk.</summary>
    public static void EncryptFile(string inputPath, string outputPath, byte[] key, byte[] iv)
    {
        using var aes = System.Security.Cryptography.Aes.Create();
        aes.Key = key;
        aes.IV = iv;

        using var input = new FileStream(inputPath, FileMode.Open, FileAccess.Read);
        using var output = new FileStream(outputPath, FileMode.Create, FileAccess.Write);
        using var encryptor = aes.CreateEncryptor();
        // Disposal order matters: CryptoStream must flush its final padded
        // block BEFORE `output` closes, so `cryptoStream` is disposed first
        // (innermost `using` exits first) — that's why it's declared last.
        using var cryptoStream = new CryptoStream(output, encryptor, CryptoStreamMode.Write);

        input.CopyTo(cryptoStream); // internally chunked; no ReadAllBytes anywhere
    }

    /// <summary>CryptoStreamMode.Read: ciphertext flows IN from disk, plaintext is what CopyTo pulls out.</summary>
    public static void DecryptFile(string inputPath, string outputPath, byte[] key, byte[] iv)
    {
        using var aes = System.Security.Cryptography.Aes.Create();
        aes.Key = key;
        aes.IV = iv;

        using var input = new FileStream(inputPath, FileMode.Open, FileAccess.Read);
        using var decryptor = aes.CreateDecryptor();
        using var cryptoStream = new CryptoStream(input, decryptor, CryptoStreamMode.Read);
        using var output = new FileStream(outputPath, FileMode.Create, FileAccess.Write);

        cryptoStream.CopyTo(output);
    }

    /// <summary>Generates a file of (approximately) the given size filled with pseudo-random bytes, in chunks.</summary>
    public static void GenerateTestFile(string path, long sizeInBytes, int chunkSize = 1024 * 1024)
    {
        using var stream = new FileStream(path, FileMode.Create, FileAccess.Write);
        var buffer = new byte[chunkSize];
        var rng = Random.Shared;

        long written = 0;
        while (written < sizeInBytes)
        {
            int toWrite = (int)Math.Min(chunkSize, sizeInBytes - written);
            rng.NextBytes(toWrite == buffer.Length ? buffer : buffer.AsSpan(0, toWrite));
            stream.Write(buffer, 0, toWrite);
            written += toWrite;
        }
    }
}
