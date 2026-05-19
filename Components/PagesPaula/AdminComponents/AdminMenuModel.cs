namespace WeddingPlannerApp.Components.PagesPaula.AdminComponents;

public sealed class AdminMenuModel
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string DietaryType { get; set; } = "standard";
    public int PricePerPerson { get; set; }
    public string Description { get; set; } = "";
    public List<AdminMenuCourseModel> Courses { get; set; } = [];
}

public sealed class AdminMenuCourseModel
{
    public string Label { get; set; } = "";
    public List<AdminMenuItemModel> Items { get; set; } = [];
}

public sealed class AdminMenuItemModel
{
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public string DietaryTagsValue { get; set; } = "";

    public List<string> DietaryTags => DietaryTagsValue
        .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
        .ToList();
}
