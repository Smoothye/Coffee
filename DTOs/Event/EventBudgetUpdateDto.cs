using System.ComponentModel.DataAnnotations;

namespace WeddingPlannerApp.DTOs.Event;

public sealed class EventBudgetUpdateDto
{
    [Range(typeof(decimal), "0", "99999999.99")]
    public decimal TotalBudget { get; set; }
}
