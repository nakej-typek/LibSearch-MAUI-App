using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LibSearch.App.Models.Entities;
using LibSearch.App.Services;

namespace LibSearch.App.ViewModels;

public partial class LibraryViewModel : ObservableObject
{
    private readonly ISessionService _session;
    private readonly ILibraryService _library;

    [ObservableProperty]
    private string _currentUsername = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsNotBusy))]
    private bool _isBusy;

    [ObservableProperty]
    private string? _statusMessage;

    public bool IsNotBusy => !IsBusy;

    public ObservableCollection<TextDocument> Documents { get; } = new();

    public LibraryViewModel(ISessionService session, ILibraryService library)
    {
        _session = session;
        _library = library;
        CurrentUsername = _session.CurrentUser?.Username ?? string.Empty;
        _session.UserChanged += (_, user) => CurrentUsername = user?.Username ?? string.Empty;
    }

    [RelayCommand]
    public async Task LoadAsync()
    {
        var user = _session.CurrentUser;
        if (user is null) return;
        Documents.Clear();
        foreach (var doc in await _library.ListAsync(user.Id))
            Documents.Add(doc);
    }

    [RelayCommand]
    private async Task UploadAsync()
    {
        var user = _session.CurrentUser;
        if (user is null || IsBusy) return;

        var picked = await FilePicker.Default.PickAsync(new PickOptions
        {
            PickerTitle = "Pick a .txt file"
        });
        if (picked is null) return;

        IsBusy = true;
        StatusMessage = $"Ingesting {picked.FileName}...";
        try
        {
            var title = Path.GetFileNameWithoutExtension(picked.FileName);
            var doc = await _library.UploadAsync(user.Id, picked.FullPath, title);
            Documents.Insert(0, doc);
            StatusMessage = null;
        }
        catch (Exception ex)
        {
            StatusMessage = $"Upload failed: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task OpenAsync(TextDocument? doc)
    {
        if (doc is null) return;
        if (doc.IngestedAt is null)
        {
            await Shell.Current.DisplayAlert("Not indexed", 
                "This document was not ingested yet — re-upload to retry.", "OK");
            return;
        }
        await Shell.Current.GoToAsync($"reader?documentId={doc.Id}");
    }

    [RelayCommand]
    private async Task RenameAsync(TextDocument? doc)
    {
        if (doc is null || IsBusy) return;
        var newTitle = await Shell.Current.DisplayPromptAsync(
            "Rename", "New title:", initialValue: doc.Title, maxLength: 200);
        if (string.IsNullOrWhiteSpace(newTitle) || newTitle == doc.Title) return;

        IsBusy = true;
        try
        {
            await _library.RenameAsync(doc.Id, newTitle.Trim());
            doc.Title = newTitle.Trim();
            var idx = Documents.IndexOf(doc);
            if (idx >= 0)
            {
                Documents.RemoveAt(idx);
                Documents.Insert(idx, doc);
            }
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task DeleteAsync(TextDocument? doc)
    {
        if (doc is null || IsBusy) return;
        var confirmed = await Shell.Current.DisplayAlert(
            "Delete", $"Delete \"{doc.Title}\"? This removes the file and its index.",
            "Delete", "Cancel");
        if (!confirmed) return;

        IsBusy = true;
        StatusMessage = "Deleting...";
        try
        {
            await _library.DeleteAsync(doc.Id);
            Documents.Remove(doc);
            StatusMessage = null;
        }
        catch (Exception ex)
        {
            StatusMessage = $"Delete failed: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task OpenSavedAsync()
    {
        await Shell.Current.GoToAsync("saved");
    }

    [RelayCommand]
    private async Task OpenHistoryAsync()
    {
        await Shell.Current.GoToAsync("history");
    }

    [RelayCommand]
    private async Task OpenStatsAsync()
    {
        await Shell.Current.GoToAsync("stats");
    }

    [RelayCommand]
    private async Task LogoutAsync()
    {
        _session.Clear();
        Documents.Clear();
        await Shell.Current.GoToAsync("//login");
    }
}
