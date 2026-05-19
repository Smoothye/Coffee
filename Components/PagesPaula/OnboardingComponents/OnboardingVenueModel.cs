namespace WeddingPlannerApp.Components.PagesPaula.OnboardingComponents;

public sealed class OnboardingVenueModel
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Image { get; set; } = "";
    public string Type { get; set; } = "";
    public int MinCapacity { get; set; }
    public int MaxCapacity { get; set; }
}
