using LibSearch.App.Data;
using LibSearch.App.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace LibSearch.App.Services;

public class SavedPassageService : ISavedPassageService
{
    private readonly AppDbContext _db;

    public SavedPassageService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<SavedPassage> SaveAsync(int ownerId, int documentId, string excerpt, string note, IEnumerable<string> tagNames, CancellationToken ct = default)
    {
        var passage = new SavedPassage
        {
            OwnerId = ownerId,
            DocumentId = documentId,
            Excerpt = excerpt,
            Note = note ?? string.Empty,
            CreatedAt = DateTime.UtcNow
        };
        _db.SavedPassages.Add(passage);
        await _db.SaveChangesAsync(ct).ConfigureAwait(false);

        await AttachTagsAsync(passage.Id, ownerId, tagNames, ct).ConfigureAwait(false);
        return passage;
    }

    public async Task<IReadOnlyList<SavedPassage>> ListAsync(int ownerId, CancellationToken ct = default)
    {
        return await _db.SavedPassages
            .Include(p => p.Document)
            .Include(p => p.Tags).ThenInclude(t => t.Tag)
            .Where(p => p.OwnerId == ownerId)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync(ct)
            .ConfigureAwait(false);
    }

    public async Task<IReadOnlyList<Tag>> ListTagsAsync(int ownerId, CancellationToken ct = default)
    {
        return await _db.Tags
            .Where(t => t.OwnerId == ownerId)
            .OrderBy(t => t.Name)
            .ToListAsync(ct)
            .ConfigureAwait(false);
    }

    public async Task UpdateAsync(int passageId, string note, IEnumerable<string> tagNames, CancellationToken ct = default)
    {
        var passage = await _db.SavedPassages
            .Include(p => p.Tags)
            .FirstOrDefaultAsync(p => p.Id == passageId, ct)
            .ConfigureAwait(false);
        if (passage is null) return;

        passage.Note = note ?? string.Empty;
        _db.SavedPassageTags.RemoveRange(passage.Tags);
        await _db.SaveChangesAsync(ct).ConfigureAwait(false);

        await AttachTagsAsync(passage.Id, passage.OwnerId, tagNames, ct).ConfigureAwait(false);
    }

    public async Task DeleteAsync(int passageId, CancellationToken ct = default)
    {
        var passage = await _db.SavedPassages.FirstOrDefaultAsync(p => p.Id == passageId, ct).ConfigureAwait(false);
        if (passage is null) return;
        _db.SavedPassages.Remove(passage);
        await _db.SaveChangesAsync(ct).ConfigureAwait(false);
    }

    public IReadOnlyList<string> ParseTags(string raw)
    {
        if (string.IsNullOrWhiteSpace(raw)) return Array.Empty<string>();
        return raw.Split(',')
            .Select(s => s.Trim())
            .Where(s => s.Length > 0)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private async Task AttachTagsAsync(int passageId, int ownerId, IEnumerable<string> tagNames, CancellationToken ct)
    {
        var names = tagNames.Select(n => n.Trim()).Where(n => n.Length > 0).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
        if (names.Count == 0) return;

        var existing = await _db.Tags
            .Where(t => t.OwnerId == ownerId && names.Contains(t.Name))
            .ToListAsync(ct)
            .ConfigureAwait(false);
        var existingMap = existing.ToDictionary(t => t.Name, StringComparer.OrdinalIgnoreCase);

        foreach (var name in names)
        {
            if (!existingMap.TryGetValue(name, out var tag))
            {
                tag = new Tag { OwnerId = ownerId, Name = name };
                _db.Tags.Add(tag);
                await _db.SaveChangesAsync(ct).ConfigureAwait(false);
                existingMap[name] = tag;
            }
            _db.SavedPassageTags.Add(new SavedPassageTag { SavedPassageId = passageId, TagId = tag.Id });
        }
        await _db.SaveChangesAsync(ct).ConfigureAwait(false);
    }
}
