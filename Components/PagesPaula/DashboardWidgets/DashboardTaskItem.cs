namespace WeddingPlannerApp.Components.PagesPaula.DashboardWidgets;

public sealed record DashboardTaskItem(int Id, string Title, string Category, DateTime DueDate, string Priority, bool Done = false)
{
    public bool IsUrgent => !Done && (DueDate - DateTime.Today).Days <= 14;
    public bool Done { get; set; } = Done;
}
