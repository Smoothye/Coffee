namespace WeddingPlannerApp.Services;

public class AppVenue
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Type { get; set; } = "Garden";
    public string Address { get; set; } = "";
    public string Description { get; set; } = "";
    public int MinCapacity { get; set; }
    public int MaxCapacity { get; set; }
    public decimal EstimatedPrice { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public string ImagePath { get; set; } = "";
    public string ImageFolder { get; set; } = "";
    public List<string> Images { get; set; } = new();
    public List<string> Tags { get; set; } = new();
    public List<string> Amenities { get; set; } = new();
}
