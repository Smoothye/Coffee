using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.Build.Framework;
using Microsoft.EntityFrameworkCore;
using WeddingPlannerApp.Data;

namespace WeddingPlannerApp.Models;

[PrimaryKey(nameof(UserId), nameof(EventId))]
public class UserEvent
{
    public int UserId { get; set; }
    [Required, ForeignKey(nameof(UserId))]
    public required ApplicationUser User { get; set; }
    
    public int EventId { get; set; }
    [Required, ForeignKey(nameof(EventId))]
    public required Event Event { get; set; }
}