using LibSearch.App.Models.Entities;

namespace LibSearch.App.ViewModels;

public class SavedPassageVm
{
    public required SavedPassage Entity { get; init; }
    public string DocumentTitle => Entity.Document?.Title ?? string.Empty;
    public string TagsDisplay => string.Join(", ", Entity.Tags.Select(t => t.Tag.Name));
}
