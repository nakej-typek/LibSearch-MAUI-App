using LibSearch.App.Data;
using Microsoft.EntityFrameworkCore;

namespace LibSearch.App.Services;

public class StatsService : IStatsService
{
    private readonly AppDbContext _db;

    public StatsService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<StatsSnapshot> GetAsync(int ownerId, CancellationToken ct = default)
    {
        var todayStart = DateTime.UtcNow.Date;
        var tomorrowStart = todayStart.AddDays(1);

        var total = await _db.SearchHistory
            .Where(h => h.OwnerId == ownerId)
            .CountAsync(ct)
            .ConfigureAwait(false);

        var today = await _db.SearchHistory
            .Where(h => h.OwnerId == ownerId && h.ExecutedAt >= todayStart && h.ExecutedAt < tomorrowStart)
            .CountAsync(ct)
            .ConfigureAwait(false);

        var saved = await _db.SavedPassages
            .Where(p => p.OwnerId == ownerId)
            .CountAsync(ct)
            .ConfigureAwait(false);

        var top = await _db.SearchHistory
            .Where(h => h.OwnerId == ownerId)
            .GroupBy(h => h.DocumentId)
            .Select(g => new { DocumentId = g.Key, Count = g.Count() })
            .OrderByDescending(x => x.Count)
            .Take(5)
            .Join(_db.Documents, g => g.DocumentId, d => d.Id, (g, d) => new TopDocument(d.Title, g.Count))
            .ToListAsync(ct)
            .ConfigureAwait(false);

        return new StatsSnapshot(total, today, saved, top);
    }
}
