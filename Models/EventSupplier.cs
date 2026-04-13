using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.Build.Framework;
using Microsoft.EntityFrameworkCore;

namespace WeddingPlannerApp.Models;

[PrimaryKey(nameof(EventId),nameof(SupplierId))]
public class EventSupplier
{
    public int EventId { get; set; }
    [Required, ForeignKey(nameof(EventId))]
    public required Event Event { get; set; }
    
    public int SupplierId { get; set; }
    [Required, ForeignKey(nameof(SupplierId))]
    public required Supplier Supplier { get; set; }

    public string? CollaborationStatus { get; set; }
    public string? Notes { get; set; }
    
}