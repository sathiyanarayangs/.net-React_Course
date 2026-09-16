using System.Security.Cryptography;
using SecureFileVault.Core.FileIO;
using SecureFileVault.Core.Streaming;
using Xunit;

namespace SecureFileVault.Tests;

public class GcmStreamVaultTests : IDisposable
{
    private readonly string _tempDir;

    public GcmStreamVaultTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), "sfv-tests-" + Guid.NewGuid());
        Directory.CreateDirectory(_tempDir);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDir)) Directory.Delete(_tempDir, recursive: true);
    }

    private string PathIn(string name) => Path.Combine(_tempDir, name);

    [Fact]
    public void EncryptThenDecrypt_SmallFile_IsByteIdenticalToOriginal()
    {
        string input = PathIn("plain.bin");
        string encrypted = PathIn("plain.sfv");
        string decrypted = PathIn("plain.dec");

        byte[] content = RandomNumberGenerator.GetBytes(5000); // smaller than one 64KB chunk
        File.WriteAllBytes(input, content);

        GcmStreamVault.EncryptFile(input, encrypted, "vault-password");
        GcmStreamVault.DecryptFile(encrypted, decrypted, "vault-password");

        Assert.True(ChunkedFileCopier.FilesAreByteIdentical(input, decrypted));
    }

    [Fact]
    public void EncryptThenDecrypt_MultiChunkFile_IsByteIdenticalToOriginal()
    {
        string input = PathIn("plain.bin");
        string encrypted = PathIn("plain.sfv");
        string decrypted = PathIn("plain.dec");

        // A few chunks' worth, using a small chunk size so the test stays fast
        // while still exercising the multi-chunk / chunk-counter path.
        const int chunkSize = 1024;
        byte[] content = RandomNumberGenerator.GetBytes(chunkSize * 5 + 37); // 5 full chunks + a partial one
        File.WriteAllBytes(input, content);

        GcmStreamVault.EncryptFile(input, encrypted, "vault-password", chunkSize);
        GcmStreamVault.DecryptFile(encrypted, decrypted, "vault-password");

        Assert.True(ChunkedFileCopier.FilesAreByteIdentical(input, decrypted));
    }

    [Fact]
    public void Decrypt_TamperedChunk_ThrowsCryptographicException()
    {
        string input = PathIn("plain.bin");
        string encrypted = PathIn("plain.sfv");
        string decrypted = PathIn("plain.dec");

        File.WriteAllBytes(input, RandomNumberGenerator.GetBytes(2000));
        GcmStreamVault.EncryptFile(input, encrypted, "vault-password");

        // Flip one byte somewhere past the fixed-size header (magic+salt+noncePrefix+chunkSize == 4+16+4+4 = 28 bytes).
        byte[] bytes = File.ReadAllBytes(encrypted);
        bytes[^1] ^= 0x01;
        File.WriteAllBytes(encrypted, bytes);

        Assert.ThrowsAny<CryptographicException>(() => GcmStreamVault.DecryptFile(encrypted, decrypted, "vault-password"));
    }

    [Fact]
    public void Decrypt_WrongPassword_ThrowsCryptographicException()
    {
        string input = PathIn("plain.bin");
        string encrypted = PathIn("plain.sfv");
        string decrypted = PathIn("plain.dec");

        File.WriteAllBytes(input, RandomNumberGenerator.GetBytes(2000));
        GcmStreamVault.EncryptFile(input, encrypted, "vault-password");

        Assert.ThrowsAny<CryptographicException>(() => GcmStreamVault.DecryptFile(encrypted, decrypted, "wrong-password"));
    }

    [Fact]
    public void Decrypt_NotAVaultFile_ThrowsInvalidDataException()
    {
        string notAVault = PathIn("random.bin");
        string decrypted = PathIn("random.dec");
        File.WriteAllBytes(notAVault, RandomNumberGenerator.GetBytes(100));

        Assert.Throws<InvalidDataException>(() => GcmStreamVault.DecryptFile(notAVault, decrypted, "any-password"));
    }
}
