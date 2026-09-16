using SecureFileVault.Core.Auth;
using Xunit;

namespace SecureFileVault.Tests;

public class PasswordHasherTests
{
    [Fact]
    public void Verify_ReturnsTrue_ForCorrectPassword()
    {
        string hash = PasswordHasher.Hash("TeacherPass123!");
        Assert.True(PasswordHasher.Verify("TeacherPass123!", hash));
    }

    [Fact]
    public void Verify_ReturnsFalse_ForWrongPassword()
    {
        string hash = PasswordHasher.Hash("TeacherPass123!");
        Assert.False(PasswordHasher.Verify("wrong-password", hash));
    }

    [Fact]
    public void Hash_ProducesDifferentOutput_ForSamePassword_OnEachCall()
    {
        // Different random salt each time -> different stored hash string,
        // even for the identical password.
        string hashA = PasswordHasher.Hash("SamePassword1!");
        string hashB = PasswordHasher.Hash("SamePassword1!");

        Assert.NotEqual(hashA, hashB);
        Assert.True(PasswordHasher.Verify("SamePassword1!", hashA));
        Assert.True(PasswordHasher.Verify("SamePassword1!", hashB));
    }

    [Fact]
    public void Hash_StoresIterationsAndSaltAlongsideHash()
    {
        string hash = PasswordHasher.Hash("SomePassword1!", iterations: 50_000);
        string[] parts = hash.Split('.');

        Assert.Equal(3, parts.Length);
        Assert.Equal("50000", parts[0]);
    }

    [Fact]
    public void Verify_ThrowsFormatException_ForMalformedStoredHash()
    {
        Assert.Throws<FormatException>(() => PasswordHasher.Verify("password", "not-a-valid-stored-hash"));
    }
}
