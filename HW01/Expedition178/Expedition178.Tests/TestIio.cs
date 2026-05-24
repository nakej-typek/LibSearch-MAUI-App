using Expedition178.IO;

namespace Expedition178.Tests;

public class TestIio : IIO
{
    public List<string> Output { get; } = [];

    public void WriteLine(string message)
    {
        Output.Add(message);
    }

    public void Log(string message)
    {
        Output.Add(message);
    }

    public string? GetMsg()
    {
        return null;
    }

    public void StartArgsParser(out int a, out int b, out int c, int max)
    {
        throw new NotImplementedException();
    }

    public void SortArgsParser(out int a, out int b, out int c, int max)
    {
        throw new NotImplementedException();
    }
}

