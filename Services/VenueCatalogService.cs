namespace WeddingPlannerApp.Services;

public class VenueCatalogService
{
    public event Action? OnChange;
    void Notify() => OnChange?.Invoke();

    private readonly List<AppVenue> _venues = new()
    {
        new AppVenue { Id=1, Name="Rosewood Gardens", Type="Garden", Address="12 Rose Lane, Greenfield", Description="A stunning garden venue surrounded by lush greenery, blooming roses and elegant fountains.", MinCapacity=50, MaxCapacity=200, EstimatedPrice=5000, Latitude=44.4268, Longitude=26.1025, ImagePath="https://images.unsplash.com/photo-1464366400600-7168b8af9bc3?w=800&q=80", Images=new(){"https://images.unsplash.com/photo-1464366400600-7168b8af9bc3?w=800&q=80","https://images.unsplash.com/photo-1519167758481-83f550bb49b3?w=800&q=80","https://images.unsplash.com/photo-1478146059778-26028b07395a?w=800&q=80","https://images.unsplash.com/photo-1510076857177-7470076d4098?w=800&q=80"}, Amenities=new(){"Parking","Catering Kitchen","Bridal Suite","Sound System","Garden","Wheelchair Access"}, ImageFolder="rosewood-gardens" },
        new AppVenue { Id=2, Name="The Grand Ballroom", Type="Indoor", Address="88 Grand Ave, Maplewood", Description="An opulent ballroom with crystal chandeliers, marble floors and world-class catering.", MinCapacity=100, MaxCapacity=500, EstimatedPrice=12000, Latitude=44.4397, Longitude=26.0963, ImagePath="https://images.unsplash.com/photo-1519167758481-83f550bb49b3?w=800&q=80", Images=new(){"https://images.unsplash.com/photo-1519167758481-83f550bb49b3?w=800&q=80","https://images.unsplash.com/photo-1464366400600-7168b8af9bc3?w=800&q=80","https://images.unsplash.com/photo-1505236858219-8359eb29e329?w=800&q=80"}, Amenities=new(){"Valet Parking","Full Catering","Honeymoon Suite","Live Band Stage","Bar","AV System"}, ImageFolder="the-grand-ballroom" },
        new AppVenue { Id=3, Name="Lakeside Pavilion", Type="Waterfront", Address="1 Lake Road, Crystal Shores", Description="A breathtaking waterfront pavilion with panoramic lake views.", MinCapacity=30, MaxCapacity=150, EstimatedPrice=3500, Latitude=44.4667, Longitude=26.0833, ImagePath="https://images.unsplash.com/photo-1478146059778-26028b07395a?w=800&q=80", Images=new(){"https://images.unsplash.com/photo-1478146059778-26028b07395a?w=800&q=80","https://images.unsplash.com/photo-1530103862676-de8c9debad1d?w=800&q=80","https://images.unsplash.com/photo-1464366400600-7168b8af9bc3?w=800&q=80"}, Amenities=new(){"Boat Dock","Outdoor Terrace","Scenic Views","Parking","Catering","Fire Pit"}, ImageFolder="lakeside-pavilion" },
        new AppVenue { Id=4, Name="Vineyard Estate", Type="Countryside", Address="44 Vine St, Rolling Hills", Description="A romantic countryside estate among rolling vineyards. Rustic elegance meets modern comfort.", MinCapacity=80, MaxCapacity=300, EstimatedPrice=7500, Latitude=44.391, Longitude=26.053, ImagePath="https://images.unsplash.com/photo-1510076857177-7470076d4098?w=800&q=80", Images=new(){"https://images.unsplash.com/photo-1510076857177-7470076d4098?w=800&q=80","https://images.unsplash.com/photo-1505236858219-8359eb29e329?w=800&q=80","https://images.unsplash.com/photo-1519167758481-83f550bb49b3?w=800&q=80"}, Amenities=new(){"Wine Cellar","Barn Hall","Outdoor Ceremony","Farm Catering","Parking","Pet Friendly"}, ImageFolder="vineyard-estate" },
    };

    public IReadOnlyList<AppVenue> Venues => _venues;

    public void AddVenue(AppVenue venue)
    {
        venue.Id = _venues.Count > 0 ? _venues.Max(v => v.Id) + 1 : 1;
        if (venue.Images.Count == 0 && !string.IsNullOrWhiteSpace(venue.ImagePath))
            venue.Images.Add(venue.ImagePath);
        _venues.Add(venue);
        Notify();
    }

    public void RemoveVenue(int id)
    {
        var removed = _venues.RemoveAll(v => v.Id == id);
        if (removed > 0) Notify();
    }
}
