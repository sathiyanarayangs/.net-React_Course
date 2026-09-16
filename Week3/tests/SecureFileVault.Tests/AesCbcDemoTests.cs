using SecureFileVault.Core.Aes;
using SecureFileVault.Core.Kdf;
using Xunit;

namespace SecureFileVault.Tests;

public class AesCbcDemoTests
{
    private static byte[] TestKey() => Pbkdf2KeyDerivation.DeriveKey("test-password", Pbkdf2KeyDerivation.GenerateSalt());

    [Fact]
    public void RoundTrip_ReturnsOriginalPlaintext()
    {
        byte[] key = TestKey();

        var (iv, ciphertext) = AesCbcDemo.Encrypt(key, "Meet at dawn.");
        string decrypted = AesCbcDemo.Decrypt(key, iv, ciphertext);

        Assert.Equal("Meet at dawn.", decrypted);
    }

    [Fact]
    public void Encrypt_ProducesDifferentIv_OnEachCall()
    {
        byte[] key = TestKey();

        var (ivA, _) = AesCbcDemo.Encrypt(key, "Meet at dawn.");
        var (ivB, _) = AesCbcDemo.Encrypt(key, "Meet at dawn.");

        Assert.NotEqual(ivA, ivB);
    }

    [Fact]
    public void Encrypt_SamePlaintext_ProducesDifferentCiphertext_BecauseIvDiffers()
    {
        byte[] key = TestKey();

        var (_, ciphertextA) = AesCbcDemo.Encrypt(key, "Meet at dawn.");
        var (_, ciphertextB) = AesCbcDemo.Encrypt(key, "Meet at dawn.");

        Assert.NotEqual(ciphertextA, ciphertextB);
    }

    [Fact]
    public void DecryptWithWrongKeyThrows_ReturnsTrue()
    {
        byte[] correctKey = TestKey();
        byte[] wrongKey = Pbkdf2KeyDerivation.DeriveKey("different-password", Pbkdf2KeyDerivation.GenerateSalt());

        bool threw = AesCbcDemo.DecryptWithWrongKeyThrows(correctKey, wrongKey, "Meet at dawn.");

        // Not a strict 100%-of-the-time guarantee (a wrong key can occasionally
        // still produce valid padding by chance), but overwhelmingly true in
        // practice for real plaintext, which is exactly Task 3.6's point:
        // CBC's "signal" is a coincidence of padding, not a real integrity check.
        Assert.True(threw);
    }
}
