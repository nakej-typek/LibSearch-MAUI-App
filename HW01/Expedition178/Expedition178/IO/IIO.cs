namespace Expedition178.IO;

public interface IIO
{
    void Log(string message);
    string? GetMsg();
    public void StartArgsParser(out int a, out int b, out int c, int max);
    public void SortArgsParser(out int a, out int b, out int c, int max);
}