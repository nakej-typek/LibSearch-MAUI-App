namespace Expedition178.IO;

public class MyIio : IIO
{
    public void Log(string message) => Console.WriteLine(message);

    public string? GetMsg() => Console.ReadLine();

    public void SortArgsParser(out int a, out int b, out int c, int max)
    {
        ParseThreeUniqueArgs(
            out a, out b, out c, max,
            requiredCommand: null,
            errorMessage: "Please enter the numbers of your adventures1-3 in your desired order.");
    }

    public void StartArgsParser(out int a, out int b, out int c, int max)
    {
        ParseThreeUniqueArgs(
            out a, out b, out c, max,
            requiredCommand: "start",
            errorMessage: "Please enter the command \"start X Y Z\" to choose your adventurers.");
    }

    private void ParseThreeUniqueArgs(
        out int a, out int b, out int c, int max,
        string? requiredCommand,
        string errorMessage)
    {
        while (true)
        {
            var parts = (GetMsg() ?? string.Empty)
                .Split(' ', StringSplitOptions.RemoveEmptyEntries);

            var offset = 0;

            if (requiredCommand is not null)
            {
                if (parts.Length != 4 ||
                    !parts[0].Equals(requiredCommand, StringComparison.OrdinalIgnoreCase))
                {
                    Log(errorMessage);
                    continue;
                }

                offset = 1;
            }
            else
            {
                if (parts.Length != 3)
                {
                    Log(errorMessage);
                    continue;
                }
            }

            if (!int.TryParse(parts[offset], out a) ||
                !int.TryParse(parts[offset + 1], out b) ||
                !int.TryParse(parts[offset + 2], out c))
            {
                Log(errorMessage);
                continue;
            }

            if (a < 1 || b < 1 || c < 1 ||
                a > max || b > max || c > max ||
                a == b || b == c || a == c)
            {
                Log(errorMessage);
                continue;
            }

            a--;
            b--;
            c--;
            return;
        }
    }
}