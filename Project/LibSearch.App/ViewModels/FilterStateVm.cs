using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using LibSearch.App.Models.Entities;

namespace LibSearch.App.ViewModels;

public class DocumentChoice
{
    public TextDocument? Document { get; init; }
    public string Display { get; init; } = string.Empty;
}

public class TagChoice
{
    public Tag? Tag { get; init; }
    public string Display { get; init; } = string.Empty;
}

public partial class FilterStateVm : ObservableObject
{
    [ObservableProperty]
    private DocumentChoice? _selectedDocument;

    [ObservableProperty]
    private TagChoice? _selectedTag;

    [ObservableProperty]
    private string _freeText = string.Empty;

    [ObservableProperty]
    private bool _showTagFilter;

    [ObservableProperty]
    private bool _isDateFilterActive;

    [ObservableProperty]
    private DateTime _dateFrom = DateTime.Today.AddMonths(-1);

    [ObservableProperty]
    private DateTime _dateTo = DateTime.Today;

    public ObservableCollection<DocumentChoice> Documents { get; } = new();
    public ObservableCollection<TagChoice> Tags { get; } = new();

    public event EventHandler? Changed;

    public FilterStateVm()
    {
        ResetDocuments(Array.Empty<TextDocument>());
        ResetTags(Array.Empty<Tag>());
    }

    public void ResetDocuments(IEnumerable<TextDocument> docs)
    {
        Documents.Clear();
        Documents.Add(new DocumentChoice { Document = null, Display = "All documents" });
        foreach (var d in docs)
            Documents.Add(new DocumentChoice { Document = d, Display = d.Title });
        SelectedDocument = Documents[0];
    }

    public void ResetTags(IEnumerable<Tag> tags)
    {
        Tags.Clear();
        Tags.Add(new TagChoice { Tag = null, Display = "All tags" });
        foreach (var t in tags)
            Tags.Add(new TagChoice { Tag = t, Display = t.Name });
        SelectedTag = Tags[0];
    }

    partial void OnSelectedDocumentChanged(DocumentChoice? value) => RaiseChanged();
    partial void OnSelectedTagChanged(TagChoice? value) => RaiseChanged();
    partial void OnFreeTextChanged(string value) => RaiseChanged();
    partial void OnIsDateFilterActiveChanged(bool value) => RaiseChanged();
    partial void OnDateFromChanged(DateTime value) { if (IsDateFilterActive) RaiseChanged(); }
    partial void OnDateToChanged(DateTime value) { if (IsDateFilterActive) RaiseChanged(); }

    private void RaiseChanged() => Changed?.Invoke(this, EventArgs.Empty);
}
