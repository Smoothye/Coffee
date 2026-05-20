using WeddingPlannerApp.Models;

namespace WeddingPlannerApp.Components.PagesPaula.GuestComponents;

public sealed record GuestMenuOption(DietaryRequirements Value, string Label)
{
    public static readonly IReadOnlyList<GuestMenuOption> DefaultOptions =
    [
        new(DietaryRequirements.Standard, "Standard")
    ];
}
