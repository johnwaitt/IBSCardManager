using IBSCardManager.Services;
using Xunit;

namespace IBSCardManager.Tests;

public sealed class CredentialVaultServiceTests
{
    [Fact]
    public void Mask_Does_Not_Expose_Raw_Secrets()
    {
        var vault = new CredentialVaultService();
        var masked = vault.Mask("abcdef1234567890");

        Assert.DoesNotContain("abcdef", masked, StringComparison.OrdinalIgnoreCase);
        Assert.EndsWith("7890", masked);
    }

    [Fact]
    public void ProtectAndUnprotect_RoundTrips_Secret()
    {
        var vault = new CredentialVaultService();
        var encrypted = vault.Protect("top-secret-token");
        var decrypted = vault.Unprotect(encrypted);

        Assert.NotEqual("top-secret-token", encrypted);
        Assert.Equal("top-secret-token", decrypted);
    }
}
