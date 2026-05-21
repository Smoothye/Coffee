namespace WeddingPlannerApp.DTOs.Supplier;

public sealed class SupplierDto
{
    public int SupplierId { get; set; }
    public required string Name { get; set; }
    public required string Category { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public decimal Advance { get; set; }
    public decimal Remaining { get; set; }
    public bool RemainingPaid { get; set; }
    public string? CollaborationStatus { get; set; }
    public string? EventNotes { get; set; }
}
