using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WeddingPlannerApp.Models;

public class ScheduleItem
{
    [Key]
    public int ScheduleItemId { get; init; }
    
    public int EventId { get; init; }
   [Required, ForeignKey(nameof(EventId))] 
    public required Event Event { get; init; }
    
    [Required]
    public required string Title { get; set; }
    
    public DateTime? StartTime { get; set; }
    
    public int? DurationMinutes { get; set; }
    
    public string? Location { get; set; }
    
    public string? Description { get; set; }
}