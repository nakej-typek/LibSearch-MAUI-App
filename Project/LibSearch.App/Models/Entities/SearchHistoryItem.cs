namespace LibSearch.App.Models.Entities;

public class SearchHistoryItem
{
    public int Id { get; set; }
    public int OwnerId { get; set; }
    public int DocumentId { get; set; }
    public required string Prompt { get; set; }
    public DateTime ExecutedAt { get; set; }
    public int ResultCount { get; set; }

    public User Owner { get; set; } = null!;
    public TextDocument Document { get; set; } = null!;
}
