using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LibSearch.App.Services;

namespace LibSearch.App.ViewModels;

public partial class SavedListViewModel : ObservableObject
{
    private readonly ISessionService _session;
    private readonly ISavedPassageService _saved;
    private readonly ILibraryService _library;
    private readonly IExportService _export;

    private readonly List<SavedPassageVm> _all = new();

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsNotBusy))]
    private bool _isBusy;

    [ObservableProperty]
    private string? _statusMessage;

    public bool IsNotBusy => !IsBusy;

    public ObservableCollection<SavedPassageVm> Items { get; } = new();

    public FilterStateVm Filter { get; }

    public SavedListViewModel(ISessionService session, ISavedPassageService saved, ILibraryService library, IExportService export)
    {
        _session = session;
        _saved = saved;
        _library = library;
        _export = export;
        Filter = new FilterStateVm { ShowTagFilter = true };
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
            var tags = await _saved.ListTagsAsync(user.Id);
            Filter.ResetDocuments(docs);
            Filter.ResetTags(tags);

            _all.Clear();
            var list = await _saved.ListAsync(user.Id);
            foreach (var p in list)
                _all.Add(new SavedPassageVm { Entity = p });
            ApplyFilter();
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void ApplyFilter()
    {
        IEnumerable<SavedPassageVm> q = _all;
        var docId = Filter.SelectedDocument?.Document?.Id;
        if (docId is not null)
            q = q.Where(i => i.Entity.DocumentId == docId.Value);

        var tagId = Filter.SelectedTag?.Tag?.Id;
        if (tagId is not null)
            q = q.Where(i => i.Entity.Tags.Any(t => t.TagId == tagId.Value));

        if (!string.IsNullOrWhiteSpace(Filter.FreeText))
        {
            var needle = Filter.FreeText.Trim();
            q = q.Where(i =>
                i.Entity.Excerpt.Contains(needle, StringComparison.OrdinalIgnoreCase) ||
                i.Entity.Note.Contains(needle, StringComparison.OrdinalIgnoreCase) ||
                i.TagsDisplay.Contains(needle, StringComparison.OrdinalIgnoreCase));
        }

        if (Filter.IsDateFilterActive)
        {
            var from = Filter.DateFrom.Date;
            var to = Filter.DateTo.Date.AddDays(1);
            q = q.Where(i => i.Entity.CreatedAt >= from && i.Entity.CreatedAt < to);
        }

        Items.Clear();
        foreach (var i in q) Items.Add(i);
    }

    [RelayCommand]
    private async Task EditAsync(SavedPassageVm? item)
    {
        if (item is null) return;
        var note = await Shell.Current.DisplayPromptAsync("Edit note", "Note:", initialValue: item.Entity.Note);
        if (note is null) return;
        var tagsRaw = await Shell.Current.DisplayPromptAsync("Edit tags", "Comma-separated tags:", initialValue: item.TagsDisplay);
        if (tagsRaw is null) return;

        IsBusy = true;
        try
        {
            await _saved.UpdateAsync(item.Entity.Id, note, _saved.ParseTags(tagsRaw));
            await LoadAsync();
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task DeleteAsync(SavedPassageVm? item)
    {
        if (item is null) return;
        var confirmed = await Shell.Current.DisplayAlert("Delete", "Delete this saved passage?", "Delete", "Cancel");
        if (!confirmed) return;

        IsBusy = true;
        try
        {
            await _saved.DeleteAsync(item.Entity.Id);
            _all.Remove(item);
            Items.Remove(item);
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task ExportAsync()
    {
        if (Items.Count == 0)
        {
            await Shell.Current.DisplayAlert("Export", "No passages to export.", "OK");
            return;
        }
        var choice = await Shell.Current.DisplayActionSheet("Export filtered passages as", "Cancel", null, "TXT", "Markdown", "JSON");
        if (string.IsNullOrEmpty(choice) || choice == "Cancel") return;

        var format = choice switch
        {
            "Markdown" => ExportFormat.Md,
            "JSON" => ExportFormat.Json,
            _ => ExportFormat.Txt
        };

        IsBusy = true;
        try
        {
            var docs = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            var path = Path.Combine(docs, _export.DefaultFileName(format));
            var entities = Items.Select(i => i.Entity);
            await _export.ExportAsync(entities, format, path);
            await Shell.Current.DisplayAlert("Export complete", $"Saved to:\n{path}", "OK");
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Export failed", ex.Message, "OK");
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
