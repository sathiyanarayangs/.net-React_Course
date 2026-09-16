using SecureFileVault.Core.FileIO;
using SecureFileVault.Core.Fundamentals;
using Xunit;

namespace SecureFileVault.Tests;

public class ChunkedFileCopierTests : IDisposable
{
    private readonly string _tempDir;

    public ChunkedFileCopierTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), "sfv-fileio-tests-" + Guid.NewGuid());
        Directory.CreateDirectory(_tempDir);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDir)) Directory.Delete(_tempDir, recursive: true);
    }

    private string PathIn(string name) => Path.Combine(_tempDir, name);

    [Fact]
    public void WriteThenRead_RoundTripsContent()
    {
        string path = PathIn("a.txt");
        ChunkedFileCopier.WriteAllText(path, "hello world");

        Assert.Equal("hello world", ChunkedFileCopier.ReadAllTextChunked(path));
    }

    [Fact]
    public void Append_AddsToExistingContent()
    {
        string path = PathIn("a.txt");
        ChunkedFileCopier.WriteAllText(path, "line one\n");
        ChunkedFileCopier.Append(path, "line two\n");

        Assert.Equal("line one\nline two\n", ChunkedFileCopier.ReadAllTextChunked(path));
    }

    [Fact]
    public void CopyChunked_ProducesByteIdenticalFile()
    {
        string source = PathIn("source.bin");
        string destination = PathIn("dest.bin");
        File.WriteAllBytes(source, System.Security.Cryptography.RandomNumberGenerator.GetBytes(10_000));

        ChunkedFileCopier.CopyChunked(source, destination, chunkSize: 512);

        Assert.True(ChunkedFileCopier.FilesAreByteIdentical(source, destination));
    }

    [Fact]
    public void FilesAreByteIdentical_ReturnsFalse_ForDifferentContent()
    {
        string a = PathIn("a.bin");
        string b = PathIn("b.bin");
        File.WriteAllBytes(a, new byte[] { 1, 2, 3 });
        File.WriteAllBytes(b, new byte[] { 1, 2, 4 });

        Assert.False(ChunkedFileCopier.FilesAreByteIdentical(a, b));
    }

    [Fact]
    public void FilesAreByteIdentical_ReturnsFalse_ForDifferentLengths()
    {
        string a = PathIn("a.bin");
        string b = PathIn("b.bin");
        File.WriteAllBytes(a, new byte[] { 1, 2, 3 });
        File.WriteAllBytes(b, new byte[] { 1, 2, 3, 4 });

        Assert.False(ChunkedFileCopier.FilesAreByteIdentical(a, b));
    }
}

public class EncodingHashingDemoTests
{
    [Fact]
    public void Base64_RoundTrips()
    {
        string original = "correct horse battery staple";
        string encoded = EncodingHashingDemo.Base64Encode(original);
        Assert.Equal(original, EncodingHashingDemo.Base64Decode(encoded));
    }

    [Fact]
    public void Sha256Hash_IsFixedLength_RegardlessOfInputSize()
    {
        string shortHash = EncodingHashingDemo.Sha256HashHex("a");
        string longHash = EncodingHashingDemo.Sha256HashHex(new string('x', 10_000));

        Assert.Equal(64, shortHash.Length); // 32 bytes -> 64 hex chars
        Assert.Equal(64, longHash.Length);
    }

    [Fact]
    public void Sha256Hash_IsDeterministic()
    {
        Assert.Equal(EncodingHashingDemo.Sha256HashHex("same input"), EncodingHashingDemo.Sha256HashHex("same input"));
    }

    [Fact]
    public void Sha256Hash_DiffersFromBase64Encoding()
    {
        string input = "correct horse battery staple";
        Assert.NotEqual(EncodingHashingDemo.Base64Encode(input), EncodingHashingDemo.Sha256HashHex(input));
    }
}
