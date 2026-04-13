using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WeddingPlannerApp.Models;

public enum CheckListTaskStatus
{
    Pending,
    InProgress,
    Completed,
    Cancelled
}

public enum CheckListTaskPriority
{
    Low,
    Medium,
    High
}

public enum CheckListTaskCategory
{
    General,
    Venue,
    Invitations,
    Catering,
    Vendors,
    Decor,
    Travel,
    Details
}

public class CheckListTask
{
    [Key]
    public int TaskId { get; init; }
    
    public int EventId { get; init; }
    [Required, ForeignKey(nameof(EventId))]
    public required Event Event { get; init; }
    
    [Required]
    public required string Title { get; set; }
    
    public DateTime DueDate { get; set; }
    
    public CheckListTaskStatus   Status   { get; set; } = CheckListTaskStatus.Pending;
    public CheckListTaskPriority Priority { get; set; } = CheckListTaskPriority.Low;
    public CheckListTaskCategory Category { get; set; } = CheckListTaskCategory.General;
    
    public string? Notes { get; set; }
}