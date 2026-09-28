using DataAccess.Core.Auth;
using Xunit;

namespace DataAccess.Tests;

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
        string hashA = PasswordHasher.Hash("SamePassword1!");
        string hashB = PasswordHasher.Hash("SamePassword1!");

        Assert.NotEqual(hashA, hashB);
        Assert.True(PasswordHasher.Verify("SamePassword1!", hashA));
        Assert.True(PasswordHasher.Verify("SamePassword1!", hashB));
    }
}
