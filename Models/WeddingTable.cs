using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WeddingPlannerApp.Models;

public class WeddingTable
{
    [Key]
    public int TableId { get; set; }
    
    public int EventId { get; set; }
    [Required, ForeignKey(nameof(EventId))]
    public required Event Event { get; set; }
    
    public int TableNumber { get; set; }
    public int Capacity { get; set; }
    
    [MaxLength(256)]
    public string? Notes { get; set; }

    public ICollection<Guest> Guests { get; set; } = [];
}