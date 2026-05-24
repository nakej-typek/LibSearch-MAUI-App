using LibSearch.App.ViewModels;

namespace LibSearch.App.Views;

public partial class SavedListPage : ContentPage
{
    private readonly SavedListViewModel _viewModel;

    public SavedListPage(SavedListViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.LoadAsync();
    }
}
