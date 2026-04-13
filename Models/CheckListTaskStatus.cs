using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace WeddingPlannerApp.Models;

[Index(nameof(StatusName), IsUnique = true)]
public class CheckListTaskStatus
{
    [Key]
    public int StatusId { get; set; }
    
    [Required]
    public required string StatusName { get; set; }
}