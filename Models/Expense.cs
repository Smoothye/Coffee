using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WeddingPlannerApp.Models;

public enum PaymentStatus
{
    Unpaid,
    Paid,
}

public enum ExpenseCategory
{
    
}

public class Expense
{
    [Key]
    public int ExpenseId { get; init; }
    
    public int EventId { get; init; }
    [Required, ForeignKey(nameof(EventId))]
    public Event? Event { get; init; }
    
    public int SupplierId { get; init; }
    [Required, ForeignKey(nameof(SupplierId))]
    public Supplier? Supplier { get; init; }
    
    public PaymentStatus PaymentStatus { get; set; } =  PaymentStatus.Unpaid;
    
    // public required string Category { get; init; }
    
    public string? Description { get; set; }
    
    // public decimal Amount { get; init; }
    public decimal EstimatedAmount { get; init; }
    public decimal ActualAmount { get; init; }
    
    public ExpenseCategory? ExpenseCategory { get; init; }
    
    public DateTime ExpenseDate { get; init; }
    
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; init; } = DateTime.UtcNow;
}