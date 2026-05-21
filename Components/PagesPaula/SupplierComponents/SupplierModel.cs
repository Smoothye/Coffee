namespace WeddingPlannerApp.Components.PagesPaula.SupplierComponents;

public sealed class SupplierModel
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Category { get; set; } = "";
    public string Phone { get; set; } = "";
    public string Email { get; set; } = "";
    public string Description { get; set; } = "";
    public decimal Price { get; set; }
    public bool Paid { get; set; }
}
