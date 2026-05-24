namespace LibSearch.App.Models.Entities;

public class User
{
    public int Id { get; set; }
    public required string Username { get; set; }
    public required string PasswordHash { get; set; }
    public DateTime CreatedAt { get; set; }

    public List<TextDocument> Documents { get; set; } = new();
    public List<SearchHistoryItem> SearchHistory { get; set; } = new();
    public List<SavedPassage> SavedPassages { get; set; } = new();
    public List<Tag> Tags { get; set; } = new();
}
