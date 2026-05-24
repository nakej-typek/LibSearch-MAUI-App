namespace LibSearch.App.Models.Entities;

public class Tag
{
    public int Id { get; set; }
    public int OwnerId { get; set; }
    public required string Name { get; set; }

    public User Owner { get; set; } = null!;
    public List<SavedPassageTag> SavedPassages { get; set; } = new();
}
