using CrossStichDrawer.Models;

namespace CrossStichDrawer.Services;

public class ThreadColorCsvLoader
{
    public List<ThreadColor> LoadFromCsv(string csvContent)
    {
        var colors = new List<ThreadColor>();
        var lines = csvContent.Split('\n', StringSplitOptions.RemoveEmptyEntries);

        for (var i = 1; i < lines.Length; i++)
        {
            var parts = lines[i].Trim().Split(',');
            if (parts.Length < 5) continue;

            var floss = parts[0];
            var name = parts[1];
            var r = byte.Parse(parts[2]);
            var g = byte.Parse(parts[3]);
            var b = byte.Parse(parts[4]);

            colors.Add(new ThreadColor(floss, name, r, g, b));
        }

        return colors;
    }
}
