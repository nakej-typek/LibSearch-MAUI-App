using LibSearch.App.Data;
using LibSearch.App.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace LibSearch.App.Services;

public class LibraryService : ILibraryService
{
    private readonly AppDbContext _db;
    private readonly ILibSearchClient _client;

    public LibraryService(AppDbContext db, ILibSearchClient client)
    {
        _db = db;
        _client = client;
    }

    public async Task<IReadOnlyList<TextDocument>> ListAsync(int ownerId, CancellationToken ct = default)
    {
        return await _db.Documents
            .Where(d => d.OwnerId == ownerId)
            .OrderByDescending(d => d.CreatedAt)
            .ToListAsync(ct)
            .ConfigureAwait(false);
    }

    public async Task<TextDocument?> GetAsync(int documentId, CancellationToken ct = default)
    {
        return await _db.Documents
            .FirstOrDefaultAsync(d => d.Id == documentId, ct)
            .ConfigureAwait(false);
    }

    public async Task<TextDocument> UploadAsync(int ownerId, string sourcePath, string title, CancellationToken ct = default)
    {
        var text = await File.ReadAllTextAsync(sourcePath, ct).ConfigureAwait(false);

        var doc = new TextDocument
        {
            OwnerId = ownerId,
            Title = title,
            FileRelativePath = string.Empty,
            CharCount = text.Length,
            CreatedAt = DateTime.UtcNow
        };
        _db.Documents.Add(doc);
        await _db.SaveChangesAsync(ct).ConfigureAwait(false);

        var relativePath = Path.Combine("library", ownerId.ToString(), $"{doc.Id}.txt");
        var absolutePath = AbsolutePath(relativePath);
        Directory.CreateDirectory(Path.GetDirectoryName(absolutePath)!);
        await File.WriteAllTextAsync(absolutePath, text, ct).ConfigureAwait(false);

        doc.FileRelativePath = relativePath;
        await _db.SaveChangesAsync(ct).ConfigureAwait(false);

        await _client.IngestAsync(CollectionName(doc), doc.Id.ToString(), text, ct).ConfigureAwait(false);

        doc.IngestedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct).ConfigureAwait(false);
        return doc;
    }

    public async Task RenameAsync(int documentId, string newTitle, CancellationToken ct = default)
    {
        var doc = await _db.Documents.FirstOrDefaultAsync(d => d.Id == documentId, ct).ConfigureAwait(false);
        if (doc is null) return;
        doc.Title = newTitle;
        await _db.SaveChangesAsync(ct).ConfigureAwait(false);
    }

    public async Task DeleteAsync(int documentId, CancellationToken ct = default)
    {
        var doc = await _db.Documents.FirstOrDefaultAsync(d => d.Id == documentId, ct).ConfigureAwait(false);
        if (doc is null) return;

        try
        {
            await _client.DeleteCollectionAsync(CollectionName(doc), ct).ConfigureAwait(false);
        }
        catch (HttpRequestException)
        {
            // server unreachable on delete should not block local cleanup
        }

        var absolutePath = AbsolutePath(doc.FileRelativePath);
        if (File.Exists(absolutePath))
            File.Delete(absolutePath);

        _db.Documents.Remove(doc);
        await _db.SaveChangesAsync(ct).ConfigureAwait(false);
    }

    public Task<string> ReadTextAsync(TextDocument document, CancellationToken ct = default)
    {
        return File.ReadAllTextAsync(AbsolutePath(document.FileRelativePath), ct);
    }

    public string CollectionName(TextDocument document) => $"user{document.OwnerId}-doc{document.Id}";

    private static string AbsolutePath(string relative) =>
        Path.Combine(FileSystem.AppDataDirectory, relative);
}
