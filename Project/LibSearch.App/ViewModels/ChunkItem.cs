using CommunityToolkit.Mvvm.ComponentModel;

namespace LibSearch.App.ViewModels;

public partial class ChunkItem : ObservableObject
{
    public int Index { get; init; }
    public required string Text { get; init; }

    [ObservableProperty]
    private bool _isHighlighted;
}
