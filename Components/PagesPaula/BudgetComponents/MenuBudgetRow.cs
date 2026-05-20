namespace WeddingPlannerApp.Components.PagesPaula.BudgetComponents;

public sealed record MenuBudgetRow(string Name, string Type, int Guests, decimal Price, decimal Total)
{
    public string Label => $"{Name} ({Type})";
    public string GuestLabel => Guests == 1 ? "1 guest" : $"{Guests} guests";
}
