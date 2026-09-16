using System.Security.Cryptography;
using SecureFileVault.Core.Kdf;

namespace SecureFileVault.Core.Streaming;

/// <summary>
/// The SecureFileVault deliverable itself: password -> PBKDF2 -> AES-GCM,
/// applied to files of any size (including 100 MB+) without ever loading
/// the whole file into memory, AND with per-chunk tamper detection — the
/// combination Tasks 3.7-3.9 and 3.12 build toward individually.
///
/// .NET's AesGcm type only encrypts/decrypts a single complete buffer at a
/// time (it doesn't plug into CryptoStream the way CBC does), so a single
/// GCM call can't cover an arbitrarily large file without buffering it
/// whole. The standard fix — used by real streaming-AEAD schemes — is to
/// split the file into fixed-size chunks and run a fresh, independently
/// authenticated AesGcm encrypt/decrypt per chunk, so no chunk (and
/// therefore no byte of the file) is ever unauthenticated, while only ever
/// holding one chunk in memory at a time.
///
/// File format written to disk:
///   MAGIC (4 bytes "SFV1") || salt (16) || nonce prefix (4) || chunk size (4, int32 LE)
///   then, repeated per chunk until EOF:
///     plaintext length of this chunk (4, int32 LE) || tag (16) || ciphertext (that many bytes)
///
/// Per-chunk nonce = noncePrefix (4 bytes, random per file) || chunkIndex (8 bytes, big-endian counter).
/// The random prefix plus the strictly-increasing counter guarantees no
/// nonce is ever reused within a file, and a different file gets a
/// different random prefix regardless of counter overlap.
/// </summary>
public static class GcmStreamVault
{
    private static readonly byte[] Magic = "SFV1"u8.ToArray();
    private const int NoncePrefixSize = 4;
    private const int NonceCounterSize = 8; // NoncePrefixSize + NonceCounterSize == 12, AES-GCM's nonce size
    private const int TagSize = 16;
    public const int DefaultChunkSize = 64 * 1024; // 64 KB plaintext per chunk

    public static void EncryptFile(string inputPath, string outputPath, string password, int chunkSize = DefaultChunkSize)
    {
        byte[] salt = Pbkdf2KeyDerivation.GenerateSalt();
        byte[] key = Pbkdf2KeyDerivation.DeriveKey(password, salt);
        byte[] noncePrefix = RandomNumberGenerator.GetBytes(NoncePrefixSize);

        using var input = new FileStream(inputPath, FileMode.Open, FileAccess.Read);
        using var output = new FileStream(outputPath, FileMode.Create, FileAccess.Write);
        using var gcm = new AesGcm(key, TagSize);

        output.Write(Magic);
        output.Write(salt);
        output.Write(noncePrefix);
        output.Write(BitConverter.GetBytes(chunkSize));

        var plaintextChunk = new byte[chunkSize];
        ulong chunkIndex = 0;
        int bytesRead;

        while ((bytesRead = input.Read(plaintextChunk, 0, chunkSize)) > 0)
        {
            byte[] nonce = BuildNonce(noncePrefix, chunkIndex);
            byte[] ciphertext = new byte[bytesRead];
            byte[] tag = new byte[TagSize];

            gcm.Encrypt(nonce, plaintextChunk.AsSpan(0, bytesRead), ciphertext, tag);

            output.Write(BitConverter.GetBytes(bytesRead));
            output.Write(tag);
            output.Write(ciphertext);

            checked { chunkIndex++; } // overflow would mean nonce reuse — fail loudly rather than silently wrap
        }
    }

    /// <summary>
    /// Decrypts a file produced by EncryptFile. Throws
    /// AuthenticationTagMismatchException the moment ANY chunk fails its
    /// tag check — including a tamper introduced only in the very last
    /// chunk of a multi-gigabyte file — without needing to have buffered
    /// earlier chunks to detect it.
    /// </summary>
    public static void DecryptFile(string inputPath, string outputPath, string password)
    {
        using var input = new FileStream(inputPath, FileMode.Open, FileAccess.Read);

        byte[] magic = ReadExact(input, Magic.Length);
        if (!magic.AsSpan().SequenceEqual(Magic))
            throw new InvalidDataException("Not a SecureFileVault (SFV1) file.");

        byte[] salt = ReadExact(input, Pbkdf2KeyDerivation.SaltSizeBytes);
        byte[] noncePrefix = ReadExact(input, NoncePrefixSize);
        int chunkSize = BitConverter.ToInt32(ReadExact(input, sizeof(int)));

        byte[] key = Pbkdf2KeyDerivation.DeriveKey(password, salt);

        using var output = new FileStream(outputPath, FileMode.Create, FileAccess.Write);
        using var gcm = new AesGcm(key, TagSize);

        var lengthBuffer = new byte[sizeof(int)];
        ulong chunkIndex = 0;

        while (true)
        {
            int lengthBytesRead = input.Read(lengthBuffer, 0, lengthBuffer.Length);
            if (lengthBytesRead == 0) break; // clean EOF between chunks
            if (lengthBytesRead != lengthBuffer.Length)
                throw new InvalidDataException("Truncated chunk header.");

            int plaintextLength = BitConverter.ToInt32(lengthBuffer);
            byte[] tag = ReadExact(input, TagSize);
            byte[] ciphertext = ReadExact(input, plaintextLength);

            byte[] nonce = BuildNonce(noncePrefix, chunkIndex);
            byte[] plaintext = new byte[plaintextLength];

            gcm.Decrypt(nonce, ciphertext, tag, plaintext); // throws on tamper — no partial/garbage output

            output.Write(plaintext, 0, plaintext.Length);
            checked { chunkIndex++; }
        }

        _ = chunkSize; // read for format completeness / future chunk-size validation
    }

    private static byte[] BuildNonce(byte[] noncePrefix, ulong chunkIndex)
    {
        byte[] nonce = new byte[NoncePrefixSize + NonceCounterSize];
        Buffer.BlockCopy(noncePrefix, 0, nonce, 0, NoncePrefixSize);
        byte[] counterBytes = BitConverter.GetBytes(chunkIndex);
        if (BitConverter.IsLittleEndian) Array.Reverse(counterBytes); // store big-endian for a predictable counter
        Buffer.BlockCopy(counterBytes, 0, nonce, NoncePrefixSize, NonceCounterSize);
        return nonce;
    }

    private static byte[] ReadExact(Stream stream, int count)
    {
        byte[] buffer = new byte[count];
        int totalRead = 0;
        while (totalRead < count)
        {
            int read = stream.Read(buffer, totalRead, count - totalRead);
            if (read == 0) throw new EndOfStreamException("Unexpected end of file while reading SecureFileVault header/chunk.");
            totalRead += read;
        }
        return buffer;
    }
}
