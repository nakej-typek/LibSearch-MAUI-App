using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LibSearch.App.Services;

namespace LibSearch.App.ViewModels;

public partial class HistoryViewModel : ObservableObject
{
    private readonly ISessionService _session;
    private readonly ISearchHistoryService _history;
    private readonly ILibraryService _library;

    private readonly List<HistoryItemVm> _all = new();

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsNotBusy))]
    private bool _isBusy;

    public bool IsNotBusy => !IsBusy;

    public ObservableCollection<HistoryItemVm> Items { get; } = new();

    public FilterStateVm Filter { get; }

    public HistoryViewModel(ISessionService session, ISearchHistoryService history, ILibraryService library)
    {
        _session = session;
        _history = history;
        _library = library;
        Filter = new FilterStateVm();
        Filter.Changed += (_, _) => ApplyFilter();
    }

    [RelayCommand]
    public async Task LoadAsync()
    {
        var user = _session.CurrentUser;
        if (user is null) return;
        IsBusy = true;
        try
        {
            var docs = await _library.ListAsync(user.Id);
            Filter.ResetDocuments(docs);

            _all.Clear();
            var list = await _history.ListAsync(user.Id);
            foreach (var h in list)
                _all.Add(new HistoryItemVm { Entity = h });
            ApplyFilter();
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void ApplyFilter()
    {
        IEnumerable<HistoryItemVm> q = _all;
        var docId = Filter.SelectedDocument?.Document?.Id;
        if (docId is not null)
            q = q.Where(i => i.Entity.DocumentId == docId.Value);

        if (!string.IsNullOrWhiteSpace(Filter.FreeText))
        {
            var needle = Filter.FreeText.Trim();
            q = q.Where(i =>
                i.Entity.Prompt.Contains(needle, StringComparison.OrdinalIgnoreCase) ||
                i.DocumentTitle.Contains(needle, StringComparison.OrdinalIgnoreCase));
        }

        if (Filter.IsDateFilterActive)
        {
            var from = Filter.DateFrom.Date;
            var to = Filter.DateTo.Date.AddDays(1);
            q = q.Where(i => i.Entity.ExecutedAt >= from && i.Entity.ExecutedAt < to);
        }

        Items.Clear();
        foreach (var i in q) Items.Add(i);
    }

    [RelayCommand]
    private async Task RerunAsync(HistoryItemVm? item)
    {
        if (item is null) return;
        var doc = item.Entity.Document;
        if (doc is null || doc.IngestedAt is null)
        {
            await Shell.Current.DisplayAlert("Cannot rerun", "Source document is missing or not indexed.", "OK");
            return;
        }
        var prompt = Uri.EscapeDataString(item.Entity.Prompt);
        await Shell.Current.GoToAsync($"//library/reader?documentId={item.Entity.DocumentId}&prompt={prompt}");
    }

    [RelayCommand]
    private async Task BackAsync()
    {
        await Shell.Current.GoToAsync("//library");
    }
}
