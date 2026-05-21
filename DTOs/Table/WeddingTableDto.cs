namespace WeddingPlannerApp.DTOs.Table;

public sealed class WeddingTableDto
{
    public int TableId { get; set; }
    public int EventId { get; set; }
    public int TableNumber { get; set; }
    public int Capacity { get; set; }
    public bool IsHeadTable { get; set; }
    public string? Notes { get; set; }
}
