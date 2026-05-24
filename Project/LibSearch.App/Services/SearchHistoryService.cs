using LibSearch.App.Data;
using LibSearch.App.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace LibSearch.App.Services;

public class SearchHistoryService : ISearchHistoryService
{
    private readonly AppDbContext _db;

    public SearchHistoryService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<SearchHistoryItem> RecordAsync(int ownerId, int documentId, string prompt, int resultCount, CancellationToken ct = default)
    {
        var entry = new SearchHistoryItem
        {
            OwnerId = ownerId,
            DocumentId = documentId,
            Prompt = prompt,
            ExecutedAt = DateTime.UtcNow,
            ResultCount = resultCount
        };
        _db.SearchHistory.Add(entry);
        await _db.SaveChangesAsync(ct).ConfigureAwait(false);
        return entry;
    }

    public async Task<IReadOnlyList<SearchHistoryItem>> ListAsync(int ownerId, CancellationToken ct = default)
    {
        return await _db.SearchHistory
            .Include(h => h.Document)
            .Where(h => h.OwnerId == ownerId)
            .OrderByDescending(h => h.ExecutedAt)
            .ToListAsync(ct)
            .ConfigureAwait(false);
    }
}
