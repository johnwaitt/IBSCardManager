namespace IBSCardManager.Models;

public enum ChecklistStatus
{
    ChecklistUnavailable,
    ChecklistPartiallyLoaded,
    ChecklistLoaded,
    ChecklistRequiresReview,
    ChecklistImportFailed
}

public static class ChecklistStatusLabels
{
    public static string ToLabel(this ChecklistStatus status)
        => status switch
        {
            ChecklistStatus.ChecklistUnavailable => "Checklist unavailable",
            ChecklistStatus.ChecklistPartiallyLoaded => "Checklist partially loaded",
            ChecklistStatus.ChecklistLoaded => "Checklist loaded",
            ChecklistStatus.ChecklistRequiresReview => "Checklist requires review",
            ChecklistStatus.ChecklistImportFailed => "Checklist import failed",
            _ => "Checklist unavailable"
        };
}
