using System.Security.Cryptography;

namespace SecureFileVault.Core.Hashing;

/// <summary>
/// Tasks 3.10-3.11 - plain hashing proves WHAT the data is (its content);
/// a keyed HMAC proves WHO produced it too, since only someone holding the
/// key could have produced a matching HMAC. Two different files (or the
/// same file with different HMAC keys) produce different HMACs even if the
/// underlying hash algorithm and data are identical.
/// </summary>
public static class FileHasher
{
    public static byte[] Sha256File(string path)
    {
        using var stream = File.OpenRead(path);
        using var sha256 = SHA256.Create();
        return sha256.ComputeHash(stream); // ComputeHash(Stream) itself reads in internal chunks
    }

    public static byte[] Sha512File(string path)
    {
        using var stream = File.OpenRead(path);
        using var sha512 = SHA512.Create();
        return sha512.ComputeHash(stream);
    }

    public static byte[] HmacSha256File(string path, byte[] key)
    {
        using var stream = File.OpenRead(path);
        using var hmac = new HMACSHA256(key);
        return hmac.ComputeHash(stream);
    }

    /// <summary>
    /// Task 3.11 - a plain `==`/SequenceEqual comparison on two byte arrays
    /// typically short-circuits at the first mismatched byte. An attacker
    /// who can measure response time (even over a network, with enough
    /// samples) can use that early-exit timing to guess a digest one byte
    /// at a time — the "timing side-channel" the task refers to.
    /// CryptographicOperations.FixedTimeEquals always compares every byte
    /// before returning, so the time taken doesn't leak how many leading
    /// bytes matched.
    /// </summary>
    public static bool ConstantTimeEquals(byte[] a, byte[] b) =>
        CryptographicOperations.FixedTimeEquals(a, b);
}
