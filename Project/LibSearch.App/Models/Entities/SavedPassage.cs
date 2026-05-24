namespace LibSearch.App.Models.Entities;

public class SavedPassage
{
    public int Id { get; set; }
    public int OwnerId { get; set; }
    public int DocumentId { get; set; }
    public required string Excerpt { get; set; }
    public string Note { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }

    public User Owner { get; set; } = null!;
    public TextDocument Document { get; set; } = null!;
    public List<SavedPassageTag> Tags { get; set; } = new();
}
