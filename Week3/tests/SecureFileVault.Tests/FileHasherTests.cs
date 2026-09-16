using SecureFileVault.Core.Hashing;
using Xunit;

namespace SecureFileVault.Tests;

public class FileHasherTests : IDisposable
{
    private readonly string _tempDir;
    private readonly string _filePath;

    public FileHasherTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), "sfv-hash-tests-" + Guid.NewGuid());
        Directory.CreateDirectory(_tempDir);
        _filePath = Path.Combine(_tempDir, "content.txt");
        File.WriteAllText(_filePath, "Some file content for hashing.");
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDir)) Directory.Delete(_tempDir, recursive: true);
    }

    [Fact]
    public void Sha256File_IsStableAcrossCalls()
    {
        byte[] hashA = FileHasher.Sha256File(_filePath);
        byte[] hashB = FileHasher.Sha256File(_filePath);

        Assert.Equal(hashA, hashB);
        Assert.Equal(32, hashA.Length); // SHA-256 -> 256 bits -> 32 bytes
    }

    [Fact]
    public void Sha512File_IsStableAcrossCalls_AndCorrectLength()
    {
        byte[] hashA = FileHasher.Sha512File(_filePath);
        byte[] hashB = FileHasher.Sha512File(_filePath);

        Assert.Equal(hashA, hashB);
        Assert.Equal(64, hashA.Length); // SHA-512 -> 512 bits -> 64 bytes
    }

    [Fact]
    public void HmacSha256File_ChangesWhenKeyChanges()
    {
        byte[] hmacA = FileHasher.HmacSha256File(_filePath, "key-one"u8.ToArray());
        byte[] hmacB = FileHasher.HmacSha256File(_filePath, "key-two"u8.ToArray());

        Assert.NotEqual(hmacA, hmacB);
    }

    [Fact]
    public void HmacSha256File_SameKeySameFile_IsStable()
    {
        byte[] key = "shared-key"u8.ToArray();
        byte[] hmacA = FileHasher.HmacSha256File(_filePath, key);
        byte[] hmacB = FileHasher.HmacSha256File(_filePath, key);

        Assert.Equal(hmacA, hmacB);
    }

    [Fact]
    public void ConstantTimeEquals_ReturnsTrue_ForIdenticalDigests()
    {
        byte[] digest = FileHasher.Sha256File(_filePath);
        Assert.True(FileHasher.ConstantTimeEquals(digest, (byte[])digest.Clone()));
    }

    [Fact]
    public void ConstantTimeEquals_ReturnsFalse_ForDifferentDigests()
    {
        byte[] digestA = FileHasher.Sha256File(_filePath);
        byte[] digestB = (byte[])digestA.Clone();
        digestB[0] ^= 0x01;

        Assert.False(FileHasher.ConstantTimeEquals(digestA, digestB));
    }
}
