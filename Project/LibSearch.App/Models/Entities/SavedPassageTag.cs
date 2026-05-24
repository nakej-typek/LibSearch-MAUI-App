namespace LibSearch.App.Models.Entities;

public class SavedPassageTag
{
    public int SavedPassageId { get; set; }
    public int TagId { get; set; }

    public SavedPassage SavedPassage { get; set; } = null!;
    public Tag Tag { get; set; } = null!;
}
