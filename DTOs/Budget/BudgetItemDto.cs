namespace WeddingPlannerApp.DTOs.Budget;

public sealed class BudgetItemDto
{
    public int BudgetItemId { get; set; }
    public int EventId { get; set; }
    public int? SupplierId { get; set; }
    public bool IsSupplierLinked { get; set; }
    public required string Name { get; set; }
    public required string Category { get; set; }
    public int Estimated { get; set; }
    public int Actual { get; set; }
    public bool Paid { get; set; }
    public DateTime ExpenseDate { get; set; }
}
