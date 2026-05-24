namespace CrossStichDrawer.Models;

public class Pattern
{
    public int Width { get; }
    public int Height { get; }
    public PatternCell[,] Cells { get; }

    public Pattern(int width, int height)
    {
        Width = width;
        Height = height;
        Cells = new PatternCell[height, width];

        for (var row = 0; row < height; row++)
        {
            for (var column = 0; column < width; column++)
            {
                Cells[row, column] = new PatternCell(row, column);
            }
        }
    }
}
