namespace WeddingPlannerApp.Components.PagesPaula.TableComponents;

public sealed class TableGuest
{
    public int Id { get; set; }
    public string FirstName { get; set; } = "";
    public string LastName { get; set; } = "";
    public string Group { get; set; } = "other";
    public string Email { get; set; } = "";
    public string Phone { get; set; } = "";
    public int? Age { get; set; }
    public string Gender { get; set; } = "";
    public int DupIndex { get; set; }
    public bool PlusOne { get; set; }
    public bool IsPlusOne { get; set; }
    public int? PlusOneForId { get; set; }
    public int? TableId { get; set; }
    public int? SeatNumber { get; set; }
    public string Status { get; set; } = "pending";
    public string DietaryRequirements { get; set; } = "none";
    public string? Notes { get; set; }
}

public sealed class EventTable
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public List<TableGuest?> Seats { get; set; } = new();
    public int Capacity { get; set; }
    public bool IsHeadTable { get; set; }
}

public sealed class DragSource
{
    public string Type { get; set; } = "pool";
    public int TableId { get; set; }
    public int SeatIndex { get; set; }
}

public sealed record GroupColors(string BgColor, string RingColor, string TextColor, string DotColor, string SeatFrom, string SeatTo);

public sealed record SeatPos(double X, double Y);

public sealed record CsvImportResult(string Message, bool HasWarning);

public sealed record TableResizeRequest(int TableId, int Delta);
