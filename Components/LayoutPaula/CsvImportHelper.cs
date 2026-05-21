using System.Text;

namespace WeddingPlannerApp.Components.LayoutPaula;

public static class CsvImportHelper
{
    public static List<string> NonEmptyLines(string content) =>
        content.Split('\n')
            .Select(line => line.TrimEnd('\r'))
            .Where(line => !string.IsNullOrWhiteSpace(line))
            .ToList();

    public static char DetectDelimiter(string header)
    {
        var commaCount = header.Count(ch => ch == ',');
        var semicolonCount = header.Count(ch => ch == ';');
        return semicolonCount > commaCount ? ';' : ',';
    }

    public static List<string> ParseFields(string line, char delimiter)
    {
        var fields = new List<string>();
        var field = new StringBuilder();
        var inQuotes = false;

        for (var i = 0; i < line.Length; i++)
        {
            var ch = line[i];
            if (ch == '"')
            {
                if (inQuotes && i + 1 < line.Length && line[i + 1] == '"')
                {
                    field.Append('"');
                    i++;
                }
                else
                {
                    inQuotes = !inQuotes;
                }

                continue;
            }

            if (ch == delimiter && !inQuotes)
            {
                fields.Add(field.ToString());
                field.Clear();
                continue;
            }

            field.Append(ch);
        }

        fields.Add(field.ToString());
        if (fields.Count == 1 && fields[0].Contains(delimiter))
            return ParseFields(fields[0], delimiter);

        return fields;
    }

    public static bool ContainsOnlyDuplicateErrors(Dictionary<string, string> errors) =>
        errors.Values.All(error => error.Contains("already exists", StringComparison.OrdinalIgnoreCase));

    public static string BuildImportMessage(string itemLabel, int imported, int duplicateSkipped, int invalidSkipped)
    {
        if (duplicateSkipped == 0 && invalidSkipped == 0)
            return $"Successfully imported {imported} {itemLabel}(s).";

        var skippedParts = new List<string>();
        if (duplicateSkipped > 0)
            skippedParts.Add($"{duplicateSkipped} duplicate(s)");
        if (invalidSkipped > 0)
            skippedParts.Add($"{invalidSkipped} invalid row(s)");

        return $"Imported {imported} {itemLabel}(s). Skipped {string.Join(" and ", skippedParts)}.";
    }
}
