using CrossStichDrawer.ViewModel;

namespace CrossStichDrawer.View;

public partial class MainPage
{
    private MainViewModel ViewModel => (MainViewModel)BindingContext;

    public MainPage()
    {
        InitializeComponent();
        ViewModel.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(MainViewModel.CurrentPattern))
                BuildGrid();
        };
    }

    private void BuildGrid()
    {
        PatternGrid.Children.Clear();
        PatternGrid.RowDefinitions.Clear();
        PatternGrid.ColumnDefinitions.Clear();

        var pattern = ViewModel.CurrentPattern;
        if (pattern == null) return;

        const int cellSize = 30;

        for (var r = 0; r < pattern.Height; r++)
            PatternGrid.RowDefinitions.Add(new RowDefinition(cellSize));
        for (var c = 0; c < pattern.Width; c++)
            PatternGrid.ColumnDefinitions.Add(new ColumnDefinition(cellSize));

        for (var r = 0; r < pattern.Height; r++)
        {
            for (var c = 0; c < pattern.Width; c++)
            {
                var cell = pattern.Cells[r, c];
                var box = new BoxView
                {
                    Color = cell.Color?.MauiColor ?? Colors.White,
                    Margin = 1
                };

                var row = r;
                var col = c;
                var tap = new TapGestureRecognizer();
                tap.Tapped += (_, _) =>
                {
                    ViewModel.FillCell(row, col);
                    box.Color = pattern.Cells[row, col].Color?.MauiColor ?? Colors.White;
                };
                box.GestureRecognizers.Add(tap);

                PatternGrid.Add(box, c, r);
            }
        }
    }
}
