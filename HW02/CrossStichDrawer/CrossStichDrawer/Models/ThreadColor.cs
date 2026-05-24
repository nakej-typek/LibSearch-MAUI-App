namespace CrossStichDrawer.Models;

public class ThreadColor(string floss, string name, byte r, byte g, byte b)
{
    public string Floss { get; } = floss;
    private string Name { get; } = name;
    public byte R { get; } = r;
    public byte G { get; } = g;
    public byte B { get; } = b;
    public Color MauiColor { get; } = Color.FromRgb(r, g, b);

    public override string ToString() => $"{Floss} - {Name}";
}
