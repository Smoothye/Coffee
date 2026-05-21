using WeddingPlannerApp.Components.LayoutPaula;

namespace WeddingPlannerApp.Components.PagesPaula.TableComponents;

public sealed record TableCsvImportOutcome(
    IReadOnlyList<TableGuest> Guests,
    int Imported,
    int DuplicateSkipped,
    int InvalidSkipped)
{
    public int Skipped => DuplicateSkipped + InvalidSkipped;
    public bool HasWarning => Skipped > 0;

    public string Message => CsvImportHelper.BuildImportMessage("guest", Imported, DuplicateSkipped, InvalidSkipped);
}

public static class TableCsvImport
{
    public static TableCsvImportOutcome Parse(string content, IEnumerable<TableGuest> existingGuests)
    {
        var lines = CsvImportHelper.NonEmptyLines(content);
        if (lines.Count == 0)
            return new TableCsvImportOutcome(Array.Empty<TableGuest>(), 0, 0, 0);

        var delimiter = CsvImportHelper.DetectDelimiter(lines[0]);
        var header = CsvImportHelper.ParseFields(lines[0], delimiter).ToArray();
        int colFn = FindColumn(header, "FirstName", 0);
        int colLn = FindColumn(header, "LastName", 1);
        int colGroup = FindColumn(header, "Group", 2);
        int colEmail = FindColumn(header, "Email", 3);
        int colPhone = FindColumn(header, "Phone", 4);
        int colPlusOne = Array.FindIndex(header, h => h.Trim().Equals("PlusOne", StringComparison.OrdinalIgnoreCase));

        var validGroups = new[] { "family", "friends", "colleagues", "other" };
        var ownersWithPlusOne = FindImportedPlusOneOwners(lines, colFn, colLn, colPlusOne);
        var allGuests = existingGuests.ToList();
        var importedGuests = new List<TableGuest>();
        int imported = 0;
        int duplicateSkipped = 0;
        int invalidSkipped = 0;
        int counter = 0;

        foreach (var line in lines.Skip(1).Where(l => !string.IsNullOrWhiteSpace(l)))
        {
            var c = CsvImportHelper.ParseFields(line, delimiter);
            if (c.Count < 2)
            {
                invalidSkipped++;
                continue;
            }

            var firstName = c.ElementAtOrDefault(colFn)?.Trim() ?? "";
            var lastName = c.ElementAtOrDefault(colLn)?.Trim() ?? "";
            var email = c.ElementAtOrDefault(colEmail)?.Trim() ?? "";
            var phone = c.ElementAtOrDefault(colPhone)?.Trim() ?? "";
            var plusOne = IsTruthy(c.ElementAtOrDefault(colPlusOne));
            var rawGroup = c.ElementAtOrDefault(colGroup)?.Trim().ToLowerInvariant() ?? "";
            var group = validGroups.Contains(rawGroup) ? rawGroup : "other";

            if (string.IsNullOrWhiteSpace(firstName) && string.IsNullOrWhiteSpace(lastName))
            {
                invalidSkipped++;
                continue;
            }

            if (IsPlusOneRow(firstName, lastName, ownersWithPlusOne, allGuests))
            {
                duplicateSkipped++;
                continue;
            }

            if (IsDuplicate(firstName, lastName, email, phone, allGuests))
            {
                duplicateSkipped++;
                continue;
            }

            var guest = new TableGuest
            {
                Id = (int)(DateTime.UtcNow.Ticks % int.MaxValue) + counter++,
                FirstName = firstName,
                LastName = lastName,
                Group = group,
                Email = email,
                Phone = phone,
                PlusOne = plusOne,
            };

            importedGuests.Add(guest);
            allGuests.Add(guest);
            if (guest.PlusOne)
                AddPlusOneGuest(guest, importedGuests, allGuests, ref counter);

            imported++;
        }

        return new TableCsvImportOutcome(importedGuests, imported, duplicateSkipped, invalidSkipped);
    }

    static int FindColumn(string[] header, string name, int fallback)
    {
        var index = Array.FindIndex(header, h => h.Trim().Equals(name, StringComparison.OrdinalIgnoreCase));
        return index >= 0 ? index : fallback;
    }

    static HashSet<string> FindImportedPlusOneOwners(IReadOnlyList<string> lines, int colFn, int colLn, int colPlusOne)
    {
        var owners = new HashSet<string>();
        foreach (var line in lines.Skip(1).Where(l => !string.IsNullOrWhiteSpace(l)))
        {
            var delimiter = CsvImportHelper.DetectDelimiter(lines[0]);
            var c = CsvImportHelper.ParseFields(line, delimiter);
            if (!IsTruthy(c.ElementAtOrDefault(colPlusOne))) continue;

            var firstName = c.ElementAtOrDefault(colFn)?.Trim() ?? "";
            var lastName = c.ElementAtOrDefault(colLn)?.Trim() ?? "";
            if (!string.IsNullOrWhiteSpace(firstName) || !string.IsNullOrWhiteSpace(lastName))
                owners.Add(GuestKey(firstName, lastName));
        }

        return owners;
    }

    static string GuestKey(string firstName, string lastName) =>
        $"{firstName.Trim().ToLowerInvariant()}|{lastName.Trim().ToLowerInvariant()}";

    static bool IsTruthy(string? value)
    {
        var v = value?.Trim().ToLowerInvariant();
        return v is "yes" or "true" or "1" or "y";
    }

    static bool IsGeneratedPlusOneName(string firstName, string lastName) =>
        firstName.Trim().EndsWith("'s", StringComparison.OrdinalIgnoreCase) &&
        (lastName.Trim().Equals("+1", StringComparison.OrdinalIgnoreCase) ||
         lastName.Trim().Equals("plus one", StringComparison.OrdinalIgnoreCase));

    static string PlusOneOwnerFirstName(string firstName) =>
        firstName.Trim().EndsWith("'s", StringComparison.OrdinalIgnoreCase)
            ? firstName.Trim()[..^2].ToLowerInvariant()
            : firstName.Trim().ToLowerInvariant();

    static bool IsPlusOneRow(string firstName, string lastName, HashSet<string> ownersWithPlusOne, IEnumerable<TableGuest> allGuests)
    {
        if (!IsGeneratedPlusOneName(firstName, lastName))
            return false;

        var ownerFirstName = PlusOneOwnerFirstName(firstName);
        return ownersWithPlusOne.Any(k => k.StartsWith($"{ownerFirstName}|", StringComparison.OrdinalIgnoreCase)) ||
               allGuests.Any(g => g.IsPlusOne && GuestKey(g.FirstName, g.LastName) == GuestKey(firstName, lastName));
    }

    static bool IsDuplicate(string firstName, string lastName, string email, string phone, IEnumerable<TableGuest> allGuests)
    {
        var guests = allGuests.ToList();

        if (!string.IsNullOrWhiteSpace(email) &&
            guests.Any(g => !string.IsNullOrWhiteSpace(g.Email) &&
                            g.Email.Trim().Equals(email.Trim(), StringComparison.OrdinalIgnoreCase)))
            return true;

        if (!string.IsNullOrWhiteSpace(phone) &&
            guests.Any(g => !string.IsNullOrWhiteSpace(g.Phone) && g.Phone.Trim() == phone.Trim()))
            return true;

        return IsGeneratedPlusOneName(firstName, lastName) &&
               guests.Any(g => g.IsPlusOne && GuestKey(g.FirstName, g.LastName) == GuestKey(firstName, lastName));
    }

    static void AddPlusOneGuest(TableGuest guest, List<TableGuest> importedGuests, List<TableGuest> allGuests, ref int counter)
    {
        if (allGuests.Any(g => g.PlusOneForId == guest.Id))
            return;

        var plusOneFirstName = $"{guest.FirstName}'s";
        var plusOneLastName = "+1";
        if (allGuests.Any(g => g.IsPlusOne && GuestKey(g.FirstName, g.LastName) == GuestKey(plusOneFirstName, plusOneLastName)))
            return;

        var plusOneGuest = new TableGuest
        {
            Id = (int)(DateTime.UtcNow.Ticks % int.MaxValue) + counter++,
            FirstName = plusOneFirstName,
            LastName = plusOneLastName,
            Group = guest.Group,
            IsPlusOne = true,
            PlusOneForId = guest.Id,
        };

        importedGuests.Add(plusOneGuest);
        allGuests.Add(plusOneGuest);
    }
}
