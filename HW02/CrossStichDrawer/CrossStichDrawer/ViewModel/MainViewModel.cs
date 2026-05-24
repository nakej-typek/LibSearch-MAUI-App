using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CrossStichDrawer.Models;
using CrossStichDrawer.Services;

namespace CrossStichDrawer.ViewModel;

public partial class MainViewModel : ObservableObject
{
    private readonly PatternFileManager _fileManager = new(new PatternParser());

    public List<ThreadColor> AvailableColors { get; }

    [ObservableProperty]
    public partial ThreadColor? SelectedColor { get; set; }

    [ObservableProperty]
    public partial Pattern? CurrentPattern { get; set; }

    [ObservableProperty]
    public partial int GridWidth { get; set; } = 10;

    [ObservableProperty]
    public partial int GridHeight { get; set; } = 10;

    public MainViewModel()
    {
        var loader = new ThreadColorCsvLoader();
        using var stream = FileSystem.OpenAppPackageFileAsync("threadcolors_dmc_rgb.csv").Result;
        using var reader = new StreamReader(stream);
        AvailableColors = loader.LoadFromCsv(reader.ReadToEnd());
    }

    [RelayCommand]
    private void CreateNewPattern() => CurrentPattern = new Pattern(GridWidth, GridHeight);

    public void FillCell(int row, int column)
    {
        if (CurrentPattern == null || SelectedColor == null) return;
        var cell = CurrentPattern.Cells[row, column];
        cell.Color = cell.Color == SelectedColor ? null : SelectedColor;
    }

    [RelayCommand(CanExecute = nameof(HasPattern))]
    private async Task SavePattern()
    {
        var result = await FilePicker.PickAsync(new PickOptions
        {
            PickerTitle = "Select file to save pattern to",
            FileTypes = new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
            {
                { DevicePlatform.WinUI, [".csp", ".txt"] }
            })
        });

        if (result == null) return;
        _fileManager.SaveToFile(CurrentPattern!, result.FullPath);
    }

    [RelayCommand]
    private async Task LoadPattern()
    {
        var result = await FilePicker.PickAsync(new PickOptions
        {
            PickerTitle = "Select a pattern file",
            FileTypes = new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
            {
                { DevicePlatform.WinUI, [".csp", ".txt"] }
            })
        });

        if (result == null) return;
        CurrentPattern = _fileManager.LoadFromFile(result.FullPath, AvailableColors);
    }

    private bool HasPattern() => CurrentPattern != null;

    partial void OnCurrentPatternChanged(Pattern? value) => SavePatternCommand.NotifyCanExecuteChanged();
}
