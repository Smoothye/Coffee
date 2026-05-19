namespace WeddingPlannerApp.Components.PagesPaula.TableComponents;

public static class TableDisplay
{
    public static string Initials(TableGuest guest) =>
        $"{(guest.FirstName.Length > 0 ? guest.FirstName[0] : ' ')}{(guest.LastName.Length > 0 ? guest.LastName[0] : ' ')}";

    public static string DisplayName(TableGuest guest)
    {
        var name = $"{guest.FirstName} {guest.LastName}";
        return guest.DupIndex > 0 ? $"{name} #{guest.DupIndex}" : name;
    }

    public static List<SeatPos> GetSeatPositions(int capacity, double radius)
    {
        var list = new List<SeatPos>();
        for (int i = 0; i < capacity; i++)
        {
            double angle = (double)i / capacity * 2 * Math.PI - Math.PI / 2;
            list.Add(new SeatPos(Math.Cos(angle) * radius, Math.Sin(angle) * radius));
        }
        return list;
    }
}
