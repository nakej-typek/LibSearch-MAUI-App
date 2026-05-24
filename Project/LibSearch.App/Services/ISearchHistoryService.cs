using LibSearch.App.Models.Entities;

namespace LibSearch.App.Services;

public interface ISearchHistoryService
{
    Task<SearchHistoryItem> RecordAsync(int ownerId, int documentId, string prompt, int resultCount, CancellationToken ct = default);
    Task<IReadOnlyList<SearchHistoryItem>> ListAsync(int ownerId, CancellationToken ct = default);
}
