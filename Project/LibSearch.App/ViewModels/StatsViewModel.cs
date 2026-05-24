using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LibSearch.App.Services;

namespace LibSearch.App.ViewModels;

public partial class StatsViewModel : ObservableObject
{
    private readonly ISessionService _session;
    private readonly IStatsService _stats;

    [ObservableProperty]
    private int _totalQueries;

    [ObservableProperty]
    private int _queriesToday;

    [ObservableProperty]
    private int _savedPassages;

    [ObservableProperty]
    private bool _isBusy;

    public ObservableCollection<TopDocument> TopDocuments { get; } = new();

    public StatsViewModel(ISessionService session, IStatsService stats)
    {
        _session = session;
        _stats = stats;
    }

    [RelayCommand]
    public async Task LoadAsync()
    {
        var user = _session.CurrentUser;
        if (user is null) return;
        IsBusy = true;
        try
        {
            var snap = await _stats.GetAsync(user.Id);
            TotalQueries = snap.TotalQueries;
            QueriesToday = snap.QueriesToday;
            SavedPassages = snap.SavedPassages;
            TopDocuments.Clear();
            foreach (var d in snap.TopDocuments) TopDocuments.Add(d);
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task BackAsync()
    {
        await Shell.Current.GoToAsync("//library");
    }
}
