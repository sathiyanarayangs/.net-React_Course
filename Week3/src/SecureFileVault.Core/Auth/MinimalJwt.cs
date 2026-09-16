using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace SecureFileVault.Core.Auth;

/// <summary>
/// Task 3.15 - "minimal JWT issue + validate + role check", built by hand
/// (no external JWT library) so every part of the mechanism is visible:
/// header.payload.signature, each Base64Url-encoded, HMAC-SHA256 signed.
/// This is intentionally small — a real system would reach for a vetted
/// library — but the shape is the same one a library gives you.
/// </summary>
public static class MinimalJwt
{
    private const string HeaderJson = /*lang=json,strict*/ """{"alg":"HS256","typ":"JWT"}""";

    public record TokenPayload(string Subject, string Role, long ExpiresAtUnixSeconds);

    public static string Issue(string subject, string role, TimeSpan validFor, string secret)
    {
        long expiresAt = DateTimeOffset.UtcNow.Add(validFor).ToUnixTimeSeconds();

        string headerB64 = Base64UrlEncode(Encoding.UTF8.GetBytes(HeaderJson));
        string payloadJson = JsonSerializer.Serialize(new { sub = subject, role, exp = expiresAt });
        string payloadB64 = Base64UrlEncode(Encoding.UTF8.GetBytes(payloadJson));

        string signingInput = $"{headerB64}.{payloadB64}";
        string signatureB64 = Base64UrlEncode(Sign(signingInput, secret));

        return $"{signingInput}.{signatureB64}";
    }

    /// <summary>
    /// Validates the signature and expiry, and returns the payload if valid.
    /// Returns false (rather than throwing) for any malformed/invalid/expired
    /// token — callers treat "not authenticated" as a normal outcome, not
    /// an exceptional one.
    /// </summary>
    public static bool TryValidate(string token, string secret, out TokenPayload? payload)
    {
        payload = null;
        string[] parts = token.Split('.');
        if (parts.Length != 3) return false;

        string signingInput = $"{parts[0]}.{parts[1]}";
        byte[] expectedSignature;
        byte[] actualSignature;
        try
        {
            expectedSignature = Sign(signingInput, secret);
            actualSignature = Base64UrlDecode(parts[2]);
        }
        catch (FormatException)
        {
            return false;
        }

        if (!CryptographicOperations.FixedTimeEquals(expectedSignature, actualSignature))
            return false; // signature invalid or tampered — reject before even looking at claims

        JsonElement root;
        try
        {
            root = JsonDocument.Parse(Base64UrlDecode(parts[1])).RootElement;
        }
        catch (Exception ex) when (ex is FormatException or JsonException)
        {
            return false;
        }

        string subject = root.GetProperty("sub").GetString() ?? "";
        string role = root.GetProperty("role").GetString() ?? "";
        long expiresAt = root.GetProperty("exp").GetInt64();

        if (DateTimeOffset.UtcNow.ToUnixTimeSeconds() > expiresAt)
            return false; // signature was valid, but the token has expired

        payload = new TokenPayload(subject, role, expiresAt);
        return true;
    }

    private static byte[] Sign(string data, string secret) =>
        HMACSHA256.HashData(Encoding.UTF8.GetBytes(secret), Encoding.UTF8.GetBytes(data));

    private static string Base64UrlEncode(byte[] bytes) =>
        Convert.ToBase64String(bytes).TrimEnd('=').Replace('+', '-').Replace('/', '_');

    private static byte[] Base64UrlDecode(string base64Url)
    {
        string base64 = base64Url.Replace('-', '+').Replace('_', '/');
        switch (base64.Length % 4)
        {
            case 2: base64 += "=="; break;
            case 3: base64 += "="; break;
        }
        return Convert.FromBase64String(base64);
    }
}
