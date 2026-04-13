using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WeddingPlannerApp.Models;

public class CheckListTask
{
    [Key]
    public int TaskId { get; set; }
    
    public int EventId { get; set; }
    [Required, ForeignKey(nameof(EventId))]
    public required Event Event { get; set; }
    
    public int StatusId { get; set; }
    [Required, ForeignKey(nameof(StatusId))]
    public required CheckListTaskStatus CheckListTaskStatus { get; set; }
    
    [Required]
    public required string Title { get; set; }
    
    [Required]
    public required string Category { get; set; }
    
    public DateTime DueDate { get; set; }
    
    public int Priority { get; set; }
    
    public string? Notes { get; set; }
}