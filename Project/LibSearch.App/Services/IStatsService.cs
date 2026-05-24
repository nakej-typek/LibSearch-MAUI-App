namespace LibSearch.App.Services;

public record TopDocument(string Title, int QueryCount);

public record StatsSnapshot(int TotalQueries, int QueriesToday, int SavedPassages, IReadOnlyList<TopDocument> TopDocuments);

public interface IStatsService
{
    Task<StatsSnapshot> GetAsync(int ownerId, CancellationToken ct = default);
}
