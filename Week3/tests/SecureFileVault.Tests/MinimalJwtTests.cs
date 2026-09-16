using SecureFileVault.Core.Auth;
using Xunit;

namespace SecureFileVault.Tests;

public class MinimalJwtTests
{
    private const string Secret = "test-secret";

    [Fact]
    public void Issue_ThenValidate_ReturnsTrue_WithCorrectClaims()
    {
        string token = MinimalJwt.Issue("alice.teacher", "Teacher", TimeSpan.FromHours(1), Secret);

        bool valid = MinimalJwt.TryValidate(token, Secret, out var payload);

        Assert.True(valid);
        Assert.Equal("alice.teacher", payload!.Subject);
        Assert.Equal("Teacher", payload.Role);
    }

    [Fact]
    public void TryValidate_ReturnsFalse_ForWrongSecret()
    {
        string token = MinimalJwt.Issue("alice.teacher", "Teacher", TimeSpan.FromHours(1), Secret);

        bool valid = MinimalJwt.TryValidate(token, "wrong-secret", out var payload);

        Assert.False(valid);
        Assert.Null(payload);
    }

    [Fact]
    public void TryValidate_ReturnsFalse_ForTamperedPayload()
    {
        string token = MinimalJwt.Issue("bob.student", "Student", TimeSpan.FromHours(1), Secret);
        string[] parts = token.Split('.');

        // Swap in a payload claiming the Teacher role, keeping the original signature.
        string forgedPayload = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(
            """{"sub":"bob.student","role":"Teacher","exp":9999999999}"""))
            .TrimEnd('=').Replace('+', '-').Replace('/', '_');
        string forgedToken = $"{parts[0]}.{forgedPayload}.{parts[2]}";

        bool valid = MinimalJwt.TryValidate(forgedToken, Secret, out var payload);

        Assert.False(valid); // signature no longer matches the (forged) payload
        Assert.Null(payload);
    }

    [Fact]
    public void TryValidate_ReturnsFalse_ForExpiredToken()
    {
        string token = MinimalJwt.Issue("alice.teacher", "Teacher", TimeSpan.FromSeconds(-1), Secret);

        bool valid = MinimalJwt.TryValidate(token, Secret, out var payload);

        Assert.False(valid);
        Assert.Null(payload);
    }

    [Theory]
    [InlineData("not-a-token")]
    [InlineData("only.two")]
    [InlineData("")]
    public void TryValidate_ReturnsFalse_ForMalformedToken(string malformed)
    {
        bool valid = MinimalJwt.TryValidate(malformed, Secret, out var payload);

        Assert.False(valid);
        Assert.Null(payload);
    }
}
