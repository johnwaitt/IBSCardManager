namespace IBSCardManager.Services;

public interface IApplicationVersionProvider
{
    string ApplicationVersion { get; }
    string InformationalVersion { get; }
}
