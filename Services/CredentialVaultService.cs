using System.Security.Cryptography;
using System.Text;

namespace IBSCardManager.Services;

public interface ICredentialVaultService
{
    string Protect(string plaintext);
    string Unprotect(string cipherText);
    string Mask(string? value);
}

public sealed class CredentialVaultService : ICredentialVaultService
{
    private static readonly byte[] Entropy = Encoding.UTF8.GetBytes("IBSCardManager-CredentialVault-v220");

    public string Protect(string plaintext)
    {
        if (string.IsNullOrWhiteSpace(plaintext)) return string.Empty;
        var bytes = Encoding.UTF8.GetBytes(plaintext);
        var protectedBytes = ProtectedData.Protect(bytes, Entropy, DataProtectionScope.CurrentUser);
        return Convert.ToBase64String(protectedBytes);
    }

    public string Unprotect(string cipherText)
    {
        if (string.IsNullOrWhiteSpace(cipherText)) return string.Empty;
        var bytes = Convert.FromBase64String(cipherText);
        var clear = ProtectedData.Unprotect(bytes, Entropy, DataProtectionScope.CurrentUser);
        return Encoding.UTF8.GetString(clear);
    }

    public string Mask(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return "Not configured";
        if (value.Length <= 4) return "****";
        return new string('*', Math.Max(4, value.Length - 4)) + value[^4..];
    }
}
