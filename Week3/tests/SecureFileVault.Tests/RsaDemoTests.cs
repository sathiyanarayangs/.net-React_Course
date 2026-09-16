using SecureFileVault.Core.Rsa;
using Xunit;

namespace SecureFileVault.Tests;

public class RsaDemoTests
{
    [Fact]
    public void EncryptThenDecrypt_ShortMessage_RoundTrips()
    {
        var (publicKey, privateKey) = RsaDemo.GenerateKeyPair();

        byte[] ciphertext = RsaDemo.EncryptShortMessage(publicKey, "The eagle lands at dusk.");
        string plaintext = RsaDemo.DecryptShortMessage(privateKey, ciphertext);

        Assert.Equal("The eagle lands at dusk.", plaintext);
    }

    [Fact]
    public void MaxPlaintextBytes_For2048BitKey_IsAround190Bytes()
    {
        int max = RsaDemo.MaxPlaintextBytes(2048);

        // 2048 bits / 8 - 2*32 - 2 = 256 - 64 - 2 = 190
        Assert.Equal(190, max);
    }
}
