namespace CrossStichDrawer.Models;

public class PatternCell(int row, int column)
{
    public int Row { get; } = row;
    public int Column { get; } = column;
    public ThreadColor? Color { get; set; }
}
