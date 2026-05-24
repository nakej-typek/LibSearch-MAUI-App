using LibSearch.App.Models.Entities;

namespace LibSearch.App.Services;

public interface ISavedPassageService
{
    Task<SavedPassage> SaveAsync(int ownerId, int documentId, string excerpt, string note, IEnumerable<string> tagNames, CancellationToken ct = default);
    Task<IReadOnlyList<SavedPassage>> ListAsync(int ownerId, CancellationToken ct = default);
    Task<IReadOnlyList<Tag>> ListTagsAsync(int ownerId, CancellationToken ct = default);
    Task UpdateAsync(int passageId, string note, IEnumerable<string> tagNames, CancellationToken ct = default);
    Task DeleteAsync(int passageId, CancellationToken ct = default);

    IReadOnlyList<string> ParseTags(string raw);
}
