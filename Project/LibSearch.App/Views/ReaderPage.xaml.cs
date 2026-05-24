using LibSearch.App.ViewModels;

namespace LibSearch.App.Views;

public partial class ReaderPage : ContentPage
{
    private readonly ReaderViewModel _viewModel;

    public ReaderPage(ReaderViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = viewModel;
        _viewModel.ScrollToChunkRequested += OnScrollToChunkRequested;
    }

    private async void OnScrollToChunkRequested(object? sender, int index)
    {
        if (index < 0 || index >= _viewModel.Chunks.Count) return;
        var item = _viewModel.Chunks[index];
        ChunksView.ScrollTo(item, position: ScrollToPosition.MakeVisible, animate: false);
        await Task.Delay(120);
        ChunksView.ScrollTo(item, position: ScrollToPosition.Start, animate: false);
        await Task.Delay(80);
        ChunksView.ScrollTo(item, position: ScrollToPosition.Start, animate: true);
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        _viewModel.ScrollToChunkRequested -= OnScrollToChunkRequested;
    }
}
