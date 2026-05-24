using LibSearch.App.Models.Entities;

namespace LibSearch.App.Services;

public interface ILibraryService
{
    Task<IReadOnlyList<TextDocument>> ListAsync(int ownerId, CancellationToken ct = default);
    Task<TextDocument?> GetAsync(int documentId, CancellationToken ct = default);
    Task<TextDocument> UploadAsync(int ownerId, string sourcePath, string title, CancellationToken ct = default);
    Task RenameAsync(int documentId, string newTitle, CancellationToken ct = default);
    Task DeleteAsync(int documentId, CancellationToken ct = default);
    Task<string> ReadTextAsync(TextDocument document, CancellationToken ct = default);
    string CollectionName(TextDocument document);
}
