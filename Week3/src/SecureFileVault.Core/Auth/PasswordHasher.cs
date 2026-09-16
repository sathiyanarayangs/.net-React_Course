using System.Security.Cryptography;
using SecureFileVault.Core.Kdf;

namespace SecureFileVault.Core.Auth;

/// <summary>
/// Task 3.15 - never store a password, store a salted PBKDF2 hash of it.
/// This is a direct reuse of Task 3.7's Pbkdf2KeyDerivation — "derive a key
/// from a password" and "hash a password for storage" are the same
/// operation; only what you do with the output differs (use it as an AES
/// key vs. compare it against a stored value).
///
/// Stored format: "{iterations}.{salt-base64}.{hash-base64}" — the
/// iteration count travels with the hash so it can be verified (and later
/// upgraded) without a separate config lookup.
/// </summary>
public static class PasswordHasher
{
    public static string Hash(string password, int iterations = Pbkdf2KeyDerivation.Iterations)
    {
        byte[] salt = Pbkdf2KeyDerivation.GenerateSalt();
        byte[] hash = Pbkdf2KeyDerivation.DeriveKey(password, salt, iterations);
        return $"{iterations}.{Convert.ToBase64String(salt)}.{Convert.ToBase64String(hash)}";
    }

    public static bool Verify(string password, string storedHash)
    {
        string[] parts = storedHash.Split('.', 3);
        if (parts.Length != 3) throw new FormatException("Stored hash is not in 'iterations.salt.hash' format.");

        int iterations = int.Parse(parts[0]);
        byte[] salt = Convert.FromBase64String(parts[1]);
        byte[] expectedHash = Convert.FromBase64String(parts[2]);

        byte[] actualHash = Pbkdf2KeyDerivation.DeriveKey(password, salt, iterations, expectedHash.Length);

        // Constant-time comparison (Task 3.11's lesson applied here, not
        // just in the hashing demo) — a login endpoint is exactly the kind
        // of externally-timeable comparison the timing side-channel targets.
        return CryptographicOperations.FixedTimeEquals(actualHash, expectedHash);
    }
}
