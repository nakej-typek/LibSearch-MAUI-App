namespace LibSearch.App.Services;

public record IngestResult(int Chunks);

public record QueryPassage(string Text, double Distance, int? ChunkIndex);

public record QueryResult(IReadOnlyList<QueryPassage> Passages);

public interface ILibSearchClient
{
    Task<bool> TestConnectionAsync(CancellationToken ct = default);
    Task<IngestResult> IngestAsync(string collection, string documentId, string text, CancellationToken ct = default);
    Task<QueryResult> QueryAsync(string collection, string prompt, CancellationToken ct = default);
    Task DeleteCollectionAsync(string collection, CancellationToken ct = default);
}
