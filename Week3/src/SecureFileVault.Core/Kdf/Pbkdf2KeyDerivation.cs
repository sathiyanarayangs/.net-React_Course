using System.Security.Cryptography;

namespace SecureFileVault.Core.Kdf;

/// <summary>
/// Task 3.7 - PBKDF2 turns a human password (low entropy, guessable) into a
/// fixed-size symmetric key. Two things make it resistant to brute force:
///
///   SALT (random, unique per user/file) — without it, an attacker could
///   precompute hashes for common passwords once (a "rainbow table") and
///   reuse that table against every user. A unique salt forces a fresh
///   computation per target.
///
///   ITERATIONS (a high, deliberately slow count) — makes each guess
///   expensive. A fast hash lets an attacker try billions of passwords a
///   second; PBKDF2 with 100k+ iterations cuts that down by orders of
///   magnitude, trading a small delay for legitimate logins for a large
///   delay for brute-force attempts.
/// </summary>
public static class Pbkdf2KeyDerivation
{
    public const int SaltSizeBytes = 16;
    public const int KeySizeBytes = 32; // 256-bit key
    public const int Iterations = 100_000;

    public static byte[] GenerateSalt() => RandomNumberGenerator.GetBytes(SaltSizeBytes);

    public static byte[] DeriveKey(string password, byte[] salt, int iterations = Iterations, int keySizeBytes = KeySizeBytes)
    {
        return Rfc2898DeriveBytes.Pbkdf2(
            password: password,
            salt: salt,
            iterations: iterations,
            hashAlgorithm: HashAlgorithmName.SHA256,
            outputLength: keySizeBytes);
    }
}
