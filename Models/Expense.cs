using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WeddingPlannerApp.Models;

public class Expense
{
    [Key]
    public int ExpenseId { get; init; }
    
    public int EventId { get; init; }
    [Required, ForeignKey(nameof(EventId))]
    public required Event Event { get; init; }
    
    public int SupplierId { get; init; }
    [Required, ForeignKey(nameof(SupplierId))]
    public required Supplier Supplier { get; init; }
    
    public int PaymentStatusId { get; init; }
    [Required, ForeignKey(nameof(PaymentStatusId))]
    public required PaymentStatus PaymentStatus { get; init; }
    
    public required string Category { get; init; }
    
    public string? Description { get; set; }
    
    public decimal Amount { get; set; }
    
    public DateTime ExpenseDate { get; set; }
    
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; init; } = DateTime.UtcNow;
}