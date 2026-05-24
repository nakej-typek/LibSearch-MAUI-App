using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LibSearch.App.Models.Entities;
using LibSearch.App.Services;

namespace LibSearch.App.ViewModels;

public partial class ReaderViewModel : ObservableObject, IQueryAttributable, IDisposable
{
    private readonly ILibraryService _library;
    private readonly ILibSearchClient _client;
    private readonly ISessionService _session;
    private readonly ISearchHistoryService _history;
    private readonly ISavedPassageService _saved;

    private CancellationTokenSource? _searchCts;
    private TextDocument? _document;
    private PassageResultVm? _pendingResult;

    [ObservableProperty]
    private int _documentId;

    [ObservableProperty]
    private string _documentTitle = string.Empty;

    [ObservableProperty]
    private string _prompt = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsNotBusy))]
    private bool _isBusy;

    [ObservableProperty]
    private string? _statusMessage;

    [ObservableProperty]
    private bool _isSaveDialogOpen;

    [ObservableProperty]
    private string _saveNote = string.Empty;

    [ObservableProperty]
    private string _saveTags = string.Empty;

    public bool IsNotBusy => !IsBusy;

    public ObservableCollection<ChunkItem> Chunks { get; } = new();
    public ObservableCollection<PassageResultVm> Results { get; } = new();

    public event EventHandler<int>? ScrollToChunkRequested;

    public ReaderViewModel(ILibraryService library, ILibSearchClient client,
        ISessionService session, ISearchHistoryService history, ISavedPassageService saved)
    {
        _library = library;
        _client = client;
        _session = session;
        _history = history;
        _saved = saved;
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("documentId", out var idObj) && int.TryParse(idObj?.ToString(), out var id))
            DocumentId = id;

        string? autoPrompt = null;
        if (query.TryGetValue("prompt", out var pObj))
        {
            autoPrompt = Uri.UnescapeDataString(pObj?.ToString() ?? string.Empty);
        }

        _ = LoadAndOptionallySearchAsync(autoPrompt);
    }

    private async Task LoadAndOptionallySearchAsync(string? autoPrompt)
    {
        await LoadDocumentAsync();
        if (!string.IsNullOrWhiteSpace(autoPrompt))
        {
            Prompt = autoPrompt;
            await SearchAsync();
        }
    }

    private async Task LoadDocumentAsync()
    {
        IsBusy = true;
        try
        {
            _document = await _library.GetAsync(DocumentId);
            if (_document is null)
            {
                StatusMessage = "Document not found.";
                return;
            }
            DocumentTitle = _document.Title;

            var text = await _library.ReadTextAsync(_document);
            var pieces = TextChunker.Chunk(text);
            Chunks.Clear();
            for (int i = 0; i < pieces.Count; i++)
                Chunks.Add(new ChunkItem { Index = i, Text = pieces[i] });
        }
        catch (Exception ex)
        {
            StatusMessage = $"Load failed: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    public async Task SearchAsync()
    {
        if (_document is null || string.IsNullOrWhiteSpace(Prompt)) return;

        _searchCts?.Cancel();
        _searchCts = new CancellationTokenSource();
        var ct = _searchCts.Token;

        IsBusy = true;
        StatusMessage = "Searching...";
        Results.Clear();
        foreach (var c in Chunks) c.IsHighlighted = false;

        try
        {
            var collection = _library.CollectionName(_document);
            var result = await _client.QueryAsync(collection, Prompt.Trim(), ct);

            for (int i = 0; i < result.Passages.Count; i++)
            {
                var p = result.Passages[i];
                Results.Add(new PassageResultVm { Text = p.Text, Distance = p.Distance, Rank = i + 1, ChunkIndex = p.ChunkIndex });
            }

            var user = _session.CurrentUser;
            if (user is not null)
                await _history.RecordAsync(user.Id, _document.Id, Prompt.Trim(), result.Passages.Count, ct);

            StatusMessage = result.Passages.Count == 0 ? "No matches." : null;
        }
        catch (OperationCanceledException)
        {
            StatusMessage = null;
        }
        catch (Exception ex)
        {
            StatusMessage = $"Search failed: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private void SelectResult(PassageResultVm? result)
    {
        if (result is null) return;
        var idx = FindChunkIndex(result.Text);
        if (idx < 0 && result.ChunkIndex is int ci && ci >= 0 && ci < Chunks.Count)
            idx = ci;
        if (idx < 0) return;
        for (int i = 0; i < Chunks.Count; i++)
            Chunks[i].IsHighlighted = i == idx;
        ScrollToChunkRequested?.Invoke(this, idx);
    }

    [RelayCommand]
    private void BeginSave(PassageResultVm? result)
    {
        if (result is null) return;
        _pendingResult = result;
        SaveNote = string.Empty;
        SaveTags = string.Empty;
        IsSaveDialogOpen = true;
    }

    [RelayCommand]
    private async Task ConfirmSaveAsync()
    {
        if (_pendingResult is null || _document is null) return;
        var user = _session.CurrentUser;
        if (user is null) return;

        IsBusy = true;
        try
        {
            var tags = _saved.ParseTags(SaveTags);
            await _saved.SaveAsync(user.Id, _document.Id, _pendingResult.Text, SaveNote, tags);
            StatusMessage = "Passage saved.";
            IsSaveDialogOpen = false;
            _pendingResult = null;
        }
        catch (Exception ex)
        {
            StatusMessage = $"Save failed: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private void CancelSave()
    {
        IsSaveDialogOpen = false;
        _pendingResult = null;
    }

    [RelayCommand]
    private async Task BackAsync()
    {
        await Shell.Current.GoToAsync("//library");
    }

    private int FindChunkIndex(string passageText)
    {
        var needle = NormalizeWhitespace(passageText);
        if (needle.Length == 0) return -1;

        for (int i = 0; i < Chunks.Count; i++)
            if (NormalizeWhitespace(Chunks[i].Text) == needle) return i;

        for (int i = 0; i < Chunks.Count; i++)
        {
            var hay = NormalizeWhitespace(Chunks[i].Text);
            if (hay.Contains(needle, StringComparison.Ordinal) || needle.Contains(hay, StringComparison.Ordinal))
                return i;
        }

        var head = needle.Length > 80 ? needle.Substring(0, 80) : needle;
        for (int i = 0; i < Chunks.Count; i++)
            if (NormalizeWhitespace(Chunks[i].Text).Contains(head, StringComparison.Ordinal))
                return i;

        return -1;
    }

    private static string NormalizeWhitespace(string s) =>
        string.Join(' ', s.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));

    public void Dispose()
    {
        _searchCts?.Cancel();
        _searchCts?.Dispose();
    }
}
