using SecureFileVault.Core.Kdf;
using Xunit;

namespace SecureFileVault.Tests;

public class Pbkdf2Tests
{
    [Fact]
    public void DeriveKey_SamePasswordAndSalt_ProducesSameKey()
    {
        byte[] salt = Pbkdf2KeyDerivation.GenerateSalt();

        byte[] keyA = Pbkdf2KeyDerivation.DeriveKey("correct-horse", salt);
        byte[] keyB = Pbkdf2KeyDerivation.DeriveKey("correct-horse", salt);

        Assert.Equal(keyA, keyB);
    }

    [Fact]
    public void DeriveKey_DifferentSalt_ProducesDifferentKey()
    {
        byte[] keyA = Pbkdf2KeyDerivation.DeriveKey("correct-horse", Pbkdf2KeyDerivation.GenerateSalt());
        byte[] keyB = Pbkdf2KeyDerivation.DeriveKey("correct-horse", Pbkdf2KeyDerivation.GenerateSalt());

        Assert.NotEqual(keyA, keyB);
    }

    [Fact]
    public void DeriveKey_DifferentPassword_ProducesDifferentKey()
    {
        byte[] salt = Pbkdf2KeyDerivation.GenerateSalt();

        byte[] keyA = Pbkdf2KeyDerivation.DeriveKey("password-one", salt);
        byte[] keyB = Pbkdf2KeyDerivation.DeriveKey("password-two", salt);

        Assert.NotEqual(keyA, keyB);
    }

    [Fact]
    public void GenerateSalt_ProducesCorrectLength()
    {
        byte[] salt = Pbkdf2KeyDerivation.GenerateSalt();
        Assert.Equal(Pbkdf2KeyDerivation.SaltSizeBytes, salt.Length);
    }

    [Fact]
    public void GenerateSalt_ProducesDifferentValuesEachCall()
    {
        byte[] saltA = Pbkdf2KeyDerivation.GenerateSalt();
        byte[] saltB = Pbkdf2KeyDerivation.GenerateSalt();
        Assert.NotEqual(saltA, saltB);
    }

    [Fact]
    public void DeriveKey_ProducesRequestedKeyLength()
    {
        byte[] salt = Pbkdf2KeyDerivation.GenerateSalt();
        byte[] key = Pbkdf2KeyDerivation.DeriveKey("password", salt, keySizeBytes: 32);
        Assert.Equal(32, key.Length);
    }
}
