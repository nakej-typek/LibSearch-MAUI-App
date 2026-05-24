using CrossStichDrawer.Models;

namespace CrossStichDrawer.Services;

public class PatternFileManager(PatternParser parser)
{
    public void SaveToFile(Pattern pattern, string filePath)
    {
        var content = parser.PatToString(pattern);
        try
        {
            File.WriteAllText(filePath, content);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
    
    public Pattern LoadFromFile(string filePath, List<ThreadColor> availableColors)
    {
        try
        {
            var content = File.ReadAllText(filePath);
            return parser.StringToPat(content, availableColors);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
}
