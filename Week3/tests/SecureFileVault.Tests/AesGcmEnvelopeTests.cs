using System.Security.Cryptography;
using SecureFileVault.Core.Aes;
using Xunit;

namespace SecureFileVault.Tests;

public class AesGcmEnvelopeTests
{
    private const string Password = "vault-password";

    [Fact]
    public void RoundTrip_CorrectPassword_ReturnsOriginalPlaintext()
    {
        byte[] envelope = AesGcmEnvelope.EncryptString(Password, "Top secret plans.");

        string decrypted = AesGcmEnvelope.DecryptToString(Password, envelope);

        Assert.Equal("Top secret plans.", decrypted);
    }

    [Fact]
    public void Encrypt_ProducesDifferentCiphertext_ForSamePlaintext_OnEachCall()
    {
        byte[] envelopeA = AesGcmEnvelope.EncryptString(Password, "same message");
        byte[] envelopeB = AesGcmEnvelope.EncryptString(Password, "same message");

        // Different salt + nonce each call -> different envelope bytes overall,
        // even though the plaintext is identical.
        Assert.NotEqual(envelopeA, envelopeB);
    }

    [Theory]
    [InlineData(0)]   // flip the first byte (part of the salt)
    [InlineData(-1)]  // flip the last byte (part of the ciphertext)
    public void Decrypt_TamperedEnvelope_ThrowsCryptographicException(int byteIndexFromEndIfNegative)
    {
        byte[] envelope = AesGcmEnvelope.EncryptString(Password, "Top secret plans.");
        byte[] tampered = (byte[])envelope.Clone();

        int index = byteIndexFromEndIfNegative >= 0
            ? byteIndexFromEndIfNegative
            : tampered.Length + byteIndexFromEndIfNegative;
        tampered[index] ^= 0x01;

        Assert.ThrowsAny<CryptographicException>(() => AesGcmEnvelope.DecryptToString(Password, tampered));
    }

    [Fact]
    public void Decrypt_TamperedTag_ThrowsCryptographicException()
    {
        byte[] envelope = AesGcmEnvelope.EncryptString(Password, "Top secret plans.");
        byte[] tampered = (byte[])envelope.Clone();

        // Tag occupies bytes [salt(16)+nonce(12) .. +16) == [28..44)
        tampered[28] ^= 0x01;

        Assert.ThrowsAny<CryptographicException>(() => AesGcmEnvelope.DecryptToString(Password, tampered));
    }

    [Fact]
    public void Decrypt_WrongPassword_ThrowsCryptographicException()
    {
        byte[] envelope = AesGcmEnvelope.EncryptString(Password, "Top secret plans.");

        Assert.ThrowsAny<CryptographicException>(() => AesGcmEnvelope.DecryptToString("wrong-password", envelope));
    }

    [Fact]
    public void Decrypt_EnvelopeTooShort_ThrowsArgumentException()
    {
        byte[] tooShort = new byte[10];

        Assert.Throws<ArgumentException>(() => AesGcmEnvelope.Decrypt(Password, tooShort));
    }
}
