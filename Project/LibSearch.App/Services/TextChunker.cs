namespace LibSearch.App.Services;

public static class TextChunker
{
    public static IReadOnlyList<string> Chunk(string text, int chunkSize = 10, int overlap = 2)
    {
        var lines = text.Split('\n')
            .Select(l => l.Trim())
            .Where(l => l.Length > 0)
            .ToList();
        if (lines.Count == 0) return Array.Empty<string>();

        var step = Math.Max(1, chunkSize - overlap);
        var chunks = new List<string>();
        for (int i = 0; i < lines.Count; i += step)
        {
            var take = Math.Min(chunkSize, lines.Count - i);
            chunks.Add(string.Join('\n', lines.Skip(i).Take(take)));
            if (i + take >= lines.Count) break;
        }
        return chunks;
    }
}
