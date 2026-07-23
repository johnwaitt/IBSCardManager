namespace IBSCardManager.Options;

public sealed class ScannerImportOptions
{
    public const string SectionName = "ScannerImport";

    public bool Enabled { get; set; }

    public string IntakeFolder { get; set; } = string.Empty;

    public string PermanentFolder { get; set; } = "uploads/cards";

    public string FailedFolder { get; set; } = "uploads/cards/failed";

    public int FileReadyDelayMilliseconds { get; set; } = 1500;
}