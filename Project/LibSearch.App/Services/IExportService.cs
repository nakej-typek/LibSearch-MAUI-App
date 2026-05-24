using LibSearch.App.Models.Entities;

namespace LibSearch.App.Services;

public enum ExportFormat
{
    Txt,
    Md,
    Json
}

public interface IExportService
{
    Task<string> ExportAsync(IEnumerable<SavedPassage> passages, ExportFormat format, string targetPath, CancellationToken ct = default);
    string DefaultFileName(ExportFormat format);
}
