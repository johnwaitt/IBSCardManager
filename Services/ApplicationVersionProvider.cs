namespace IBSCardManager.Services;

public sealed class ApplicationVersionProvider : IApplicationVersionProvider
{
    public string ApplicationVersion { get; }
    public string InformationalVersion { get; }

    public ApplicationVersionProvider()
    {
        var informationalVersion = typeof(Program).Assembly
            .GetCustomAttributes(typeof(System.Reflection.AssemblyInformationalVersionAttribute), false)
            .OfType<System.Reflection.AssemblyInformationalVersionAttribute>()
            .FirstOrDefault()?.InformationalVersion;

        InformationalVersion = string.IsNullOrWhiteSpace(informationalVersion) ? "unknown" : informationalVersion;
        ApplicationVersion = ExtractNumericVersion(InformationalVersion);
    }

    private static string ExtractNumericVersion(string value)
    {
        var normalized = value.Trim();
        var plusIndex = normalized.IndexOf('+');
        if (plusIndex >= 0)
        {
            normalized = normalized.Substring(0, plusIndex);
        }

        var dashIndex = normalized.IndexOf('-');
        if (dashIndex > 0)
        {
            normalized = normalized.Substring(0, dashIndex);
        }

        return normalized;
    }
}
