using System.Text;
using System.Text.Json;
using LibSearch.App.Models.Entities;

namespace LibSearch.App.Services;

public class ExportService : IExportService
{
    public async Task<string> ExportAsync(IEnumerable<SavedPassage> passages, ExportFormat format, string targetPath, CancellationToken ct = default)
    {
        var content = format switch
        {
            ExportFormat.Txt => BuildTxt(passages),
            ExportFormat.Md => BuildMd(passages),
            ExportFormat.Json => BuildJson(passages),
            _ => throw new ArgumentOutOfRangeException(nameof(format))
        };

        Directory.CreateDirectory(Path.GetDirectoryName(targetPath)!);
        await File.WriteAllTextAsync(targetPath, content, Encoding.UTF8, ct).ConfigureAwait(false);
        return targetPath;
    }

    public string DefaultFileName(ExportFormat format)
    {
        var stamp = DateTime.Now.ToString("yyyyMMdd-HHmm");
        var ext = format switch
        {
            ExportFormat.Txt => "txt",
            ExportFormat.Md => "md",
            ExportFormat.Json => "json",
            _ => "txt"
        };
        return $"libsearch-passages-{stamp}.{ext}";
    }

    private static string BuildTxt(IEnumerable<SavedPassage> passages)
    {
        var sb = new StringBuilder();
        foreach (var p in passages)
        {
            sb.AppendLine(p.Excerpt);
            if (!string.IsNullOrWhiteSpace(p.Note))
            {
                sb.AppendLine();
                sb.AppendLine($"Note: {p.Note}");
            }
            var tagNames = p.Tags.Select(t => t.Tag.Name).ToList();
            if (tagNames.Count > 0)
                sb.AppendLine($"Tags: {string.Join(", ", tagNames)}");
            sb.AppendLine();
            sb.AppendLine();
        }
        return sb.ToString();
    }

    private static string BuildMd(IEnumerable<SavedPassage> passages)
    {
        var sb = new StringBuilder();
        var i = 1;
        foreach (var p in passages)
        {
            var docTitle = p.Document?.Title ?? "(unknown)";
            sb.AppendLine($"## Passage {i} — {docTitle}");
            sb.AppendLine();
            sb.AppendLine($"> {p.Excerpt.Replace("\n", "\n> ")}");
            sb.AppendLine();
            if (!string.IsNullOrWhiteSpace(p.Note))
            {
                sb.AppendLine($"**Note:** {p.Note}");
                sb.AppendLine();
            }
            var tagNames = p.Tags.Select(t => t.Tag.Name).ToList();
            if (tagNames.Count > 0)
            {
                sb.AppendLine($"**Tags:** {string.Join(", ", tagNames.Select(t => $"`{t}`"))}");
                sb.AppendLine();
            }
            sb.AppendLine($"_Saved: {p.CreatedAt:yyyy-MM-dd HH:mm}_");
            sb.AppendLine();
            i++;
        }
        return sb.ToString();
    }

    private static string BuildJson(IEnumerable<SavedPassage> passages)
    {
        var dto = passages.Select(p => new
        {
            id = p.Id,
            document = p.Document?.Title,
            excerpt = p.Excerpt,
            note = p.Note,
            tags = p.Tags.Select(t => t.Tag.Name).ToArray(),
            createdAt = p.CreatedAt
        }).ToArray();
        return JsonSerializer.Serialize(dto, new JsonSerializerOptions { WriteIndented = true });
    }
}
