using LibSearch.App.Models.Entities;

namespace LibSearch.App.ViewModels;

public class HistoryItemVm
{
    public required SearchHistoryItem Entity { get; init; }
    public string DocumentTitle => Entity.Document?.Title ?? "(unknown)";
}
