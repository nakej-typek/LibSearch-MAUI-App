namespace LibSearch.App.ViewModels;

public class PassageResultVm
{
    public required string Text { get; init; }
    public double Distance { get; init; }
    public int Rank { get; init; }
    public int? ChunkIndex { get; init; }
}
