namespace WeddingPlannerApp.Components.PagesPaula.AdminComponents;

public sealed class AdminVenueModel
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Type { get; set; } = "Garden";
    public string Address { get; set; } = "";
    public int MinCapacity { get; set; }
    public int MaxCapacity { get; set; }
    public decimal EstimatedPrice { get; set; }
    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }
    public string ImagePath { get; set; } = "";
    public string ImageFolder { get; set; } = "";
    public List<string> Tags { get; set; } = new() { "Garden" };
}
