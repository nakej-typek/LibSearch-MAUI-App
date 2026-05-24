using System.Text;
using CrossStichDrawer.Models;

namespace CrossStichDrawer.Services;

public class PatternParser
{
    public string PatToString(Pattern pattern)
    {
        var stringBuilder = new StringBuilder();
        stringBuilder.AppendLine($"SIZE;{pattern.Width};{pattern.Height}");

        for (var row = 0; row < pattern.Height; row++)
        for (var column = 0; column < pattern.Width; column++)
        {
            var cell = pattern.Cells[row, column];
            if (cell.Color != null)
                stringBuilder.AppendLine($"CELL;{row};{column};{cell.Color.Floss}");
        }

        return stringBuilder.ToString();
    }

    public Pattern StringToPat(string data, List<ThreadColor> availableColors)
    {
        var colorLookup = availableColors.ToDictionary(tc => tc.Floss);
        var lines = data.Split('\n', StringSplitOptions.RemoveEmptyEntries);

        var sizeParts = lines[0].Trim().Split(';');
        if (sizeParts.Length < 3 || sizeParts[0] != "SIZE")
            throw new FormatException("Invalid pattern file: missing SIZE line.");

        var width = int.Parse(sizeParts[1]);
        var height = int.Parse(sizeParts[2]);
        var pattern = new Pattern(width, height);

        for (var i = 1; i < lines.Length; i++)
        {
            var parts = lines[i].Trim().Split(';');
            if (parts.Length < 4 || parts[0] != "CELL") continue;

            var row = int.Parse(parts[1]);
            var col = int.Parse(parts[2]);
            var floss = parts[3];

            if (colorLookup.TryGetValue(floss, out var color))
                pattern.Cells[row, col].Color = color;
        }

        return pattern;
    }
}
