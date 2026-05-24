using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace LibSearch.App.Services;

public class LibSearchHttpClient : ILibSearchClient
{
    private readonly HttpClient _http;

    public LibSearchHttpClient(HttpClient http)
    {
        _http = http;
    }

    public async Task<bool> TestConnectionAsync(CancellationToken ct = default)
    {
        try
        {
            using var resp = await _http.GetAsync("/test-connection/", ct).ConfigureAwait(false);
            return resp.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    public async Task<IngestResult> IngestAsync(string collection, string documentId, string text, CancellationToken ct = default)
    {
        var payload = new IngestPayload(collection, documentId, text);
        using var resp = await _http.PostAsJsonAsync("/ingest/", payload, ct).ConfigureAwait(false);
        resp.EnsureSuccessStatusCode();
        var body = await resp.Content.ReadFromJsonAsync<IngestResponse>(cancellationToken: ct).ConfigureAwait(false);
        return new IngestResult(body?.Chunks ?? 0);
    }

    public async Task<QueryResult> QueryAsync(string collection, string prompt, CancellationToken ct = default)
    {
        var payload = new QueryPayload(collection, prompt);
        var req = new HttpRequestMessage(HttpMethod.Get, "/query/")
        {
            Content = JsonContent.Create(payload)
        };
        using var resp = await _http.SendAsync(req, ct).ConfigureAwait(false);
        resp.EnsureSuccessStatusCode();
        using var stream = await resp.Content.ReadAsStreamAsync(ct).ConfigureAwait(false);
        var raw = await JsonSerializer.DeserializeAsync<QueryRawResponse>(stream, cancellationToken: ct).ConfigureAwait(false);
        return ToQueryResult(raw);
    }

    public async Task DeleteCollectionAsync(string collection, CancellationToken ct = default)
    {
        using var resp = await _http.DeleteAsync($"/collection/{collection}", ct).ConfigureAwait(false);
        if (resp.StatusCode == HttpStatusCode.NotFound)
            return;
        resp.EnsureSuccessStatusCode();
    }

    private static QueryResult ToQueryResult(QueryRawResponse? raw)
    {
        if (raw?.Result is null)
            return new QueryResult(Array.Empty<QueryPassage>());

        var docs = raw.Result.Documents?.FirstOrDefault();
        var dists = raw.Result.Distances?.FirstOrDefault();
        var metas = raw.Result.Metadatas?.FirstOrDefault();
        if (docs is null)
            return new QueryResult(Array.Empty<QueryPassage>());

        var list = new List<QueryPassage>(docs.Count);
        for (int i = 0; i < docs.Count; i++)
        {
            var d = dists is not null && i < dists.Count ? dists[i] : 0;
            int? chunkIndex = null;
            if (metas is not null && i < metas.Count && metas[i] is not null
                && metas[i]!.TryGetValue("chunk_index", out var ciElem))
            {
                if (ciElem.ValueKind == System.Text.Json.JsonValueKind.Number && ciElem.TryGetInt32(out var ci))
                    chunkIndex = ci;
            }
            list.Add(new QueryPassage(docs[i], d, chunkIndex));
        }
        return new QueryResult(list);
    }

    private sealed record IngestPayload(
        [property: JsonPropertyName("collection")] string Collection,
        [property: JsonPropertyName("document_id")] string DocumentId,
        [property: JsonPropertyName("text")] string Text);

    private sealed record IngestResponse([property: JsonPropertyName("chunks")] int Chunks);

    private sealed record QueryPayload(
        [property: JsonPropertyName("collection")] string Collection,
        [property: JsonPropertyName("prompt")] string Prompt);

    private sealed class QueryRawResponse
    {
        [JsonPropertyName("result")] public QueryRawBody? Result { get; set; }
    }

    private sealed class QueryRawBody
    {
        [JsonPropertyName("documents")] public List<List<string>>? Documents { get; set; }
        [JsonPropertyName("distances")] public List<List<double>>? Distances { get; set; }
        [JsonPropertyName("metadatas")] public List<List<Dictionary<string, System.Text.Json.JsonElement>?>>? Metadatas { get; set; }
    }
}
