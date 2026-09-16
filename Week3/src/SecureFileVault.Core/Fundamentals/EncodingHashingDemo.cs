using System.Security.Cryptography;
using System.Text;

namespace SecureFileVault.Core.Fundamentals;

/// <summary>
/// Task 3.2 - the one-line distinction, proven in code rather than just
/// asserted:
///
///   Encoding (Base64) is a REVERSIBLE, KEYLESS representation change —
///   anyone can decode it, so it carries no secrecy at all.
///
///   Hashing (SHA-256) is ONE-WAY and fixed-length — you cannot get the
///   input back from the digest, by design.
///
///   Encryption (see Aes/ classes) is reversible ONLY with a key — that's
///   the difference that actually provides confidentiality.
///
/// Wrong-tool examples: Base64-"encoding" a password before storing it is
/// not protection (trivially decoded); hashing a file you need to send to
/// someone and get back intact is useless (there's no "back").
/// </summary>
public static class EncodingHashingDemo
{
    public static string Base64Encode(string plainText) =>
        Convert.ToBase64String(Encoding.UTF8.GetBytes(plainText));

    public static string Base64Decode(string encoded) =>
        Encoding.UTF8.GetString(Convert.FromBase64String(encoded));

    public static string Sha256HashHex(string input)
    {
        byte[] hash = SHA256.HashData(Encoding.UTF8.GetBytes(input));
        return Convert.ToHexString(hash); // fixed-length: always 64 hex chars for SHA-256, any input size
    }
}
