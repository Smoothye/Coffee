namespace WeddingPlannerApp.Components.PagesPaula.MenuComponents;

public sealed record Dish(string Name, string Description, List<string> DietaryTags);

public sealed record Course(string Label, List<Dish> Items);

public sealed record MenuPackage(string Id, string Name, string DietaryType, int PricePerPerson, string Description, List<Course> Courses);

public sealed record DietFilter(string Key, string Label);
