namespace WeddingPlannerApp.Components.PagesPaula.GuestComponents;

public sealed class GuestModel
{
    public int Id { get; set; }
    public string FirstName { get; set; } = "";
    public string LastName { get; set; } = "";
    public string Email { get; set; } = "";
    public string Phone { get; set; } = "";
    public int? Age { get; set; }
    public string Gender { get; set; } = "";
    public string DietaryRequirements { get; set; } = "none";
    public string MenuType { get; set; } = "casual";
    public string Group { get; set; } = "friends";
    public string Status { get; set; } = "pending";
    public bool PlusOne { get; set; }
    public bool IsPlusOne { get; set; }
    public int? PlusOneForId { get; set; }
}
