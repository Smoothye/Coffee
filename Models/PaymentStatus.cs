using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
namespace WeddingPlannerApp.Models;

[Index(nameof(StatusName), IsUnique = true)]
public class PaymentStatus
{
    [Key]
    public int PaymentStatusId { get; set; }
    
    [Required]
    public required string StatusName { get; set; }
}