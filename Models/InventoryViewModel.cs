using IBSCardManager.Entities;

namespace IBSCardManager.Models;

public class InventoryViewModel
{
    public List<Card> Cards { get; set; } = new();
    public string? Search { get; set; }
    public int RecordCount { get; set; }
    public int PieceCount { get; set; }
    public decimal CollectionValue { get; set; }
    public int GradedCount { get; set; }
    public int ListedCount { get; set; }
    public List<string> Teams { get; set; } = new();
    public List<string> Brands { get; set; } = new();
    public List<int> Years { get; set; } = new();
    public List<string> Grades { get; set; } = new();
    public List<string> Statuses { get; set; } = new();
    public List<string> StorageBoxes { get; set; } = new();
}
