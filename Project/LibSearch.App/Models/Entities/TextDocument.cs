namespace LibSearch.App.Models.Entities;

public class TextDocument
{
    public int Id { get; set; }
    public int OwnerId { get; set; }
    public required string Title { get; set; }
    public required string FileRelativePath { get; set; }
    public int CharCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? IngestedAt { get; set; }

    public User Owner { get; set; } = null!;
    public List<SearchHistoryItem> SearchHistory { get; set; } = new();
    public List<SavedPassage> SavedPassages { get; set; } = new();
}
