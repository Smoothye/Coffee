namespace WeddingPlannerApp.Components.PagesPaula.AdminComponents;

public sealed class AdminEventModel
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Type { get; set; } = "Wedding";
    public DateTime EventDate { get; set; } = DateTime.Today.AddMonths(3);
    public int VenueId { get; set; }
    public string VenueName { get; set; } = "";
    public int GuestCount { get; set; }
    public int EstimatedGuests { get; set; }
    public decimal TotalBudget { get; set; }
    public string OwnerName { get; set; } = "";
    public string OwnerEmail { get; set; } = "";
    public string MenuName { get; set; } = "";
    public string Notes { get; set; } = "";
    public DateTime CreatedAt { get; set; } = DateTime.Now;
}
