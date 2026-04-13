using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.Build.Framework;
using Microsoft.EntityFrameworkCore;

namespace WeddingPlannerApp.Models;

[PrimaryKey(nameof(EventId),nameof(SupplierId))]
public class EventSupplier
{
    public int EventId { get; init; }
    [Required, ForeignKey(nameof(EventId))]
    public required Event Event { get; init; }
    
    public int SupplierId { get; init; }
    [Required, ForeignKey(nameof(SupplierId))]
    public required Supplier Supplier { get; init; }

    public string? CollaborationStatus { get; set; }
    
    public string? Notes { get; set; }
}