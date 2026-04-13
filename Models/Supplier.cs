using System.ComponentModel.DataAnnotations;

namespace WeddingPlannerApp.Models;

public class Supplier
{
    [Key]
    public int SupplierId { get; init; }
    
    [Required]
    public required string Name { get; init; }
    
    [Required]
    public required string SupplierType { get; init; }
    
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public decimal BasePrice { get; set; }
    
    public string? Notes { get; set; }

    public ICollection<Expense> Expenses { get; set; } = [];
    public ICollection<EventSupplier> EventSuppliers { get; set; } = [];
}