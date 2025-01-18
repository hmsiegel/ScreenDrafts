namespace ScreenDrafts.Modules.Drafts.Domain.Drafts;

public sealed class Draft
{
  public Guid Id { get; set; }

  public required string Title { get; set; }

  public required DraftType DraftType { get; set; }

  public required int NumberOfDrafters { get; set; }

  public required int NumberOfCommissioners { get; set; }

  public required int NumberOfMovies { get; set; }
}

public sealed class DraftType(string name, int value) 
  : SmartEnum<DraftType>(name, value)
{
  public static readonly DraftType Regular = new(nameof(Regular), 0);
  public static readonly DraftType Expanded = new(nameof(Expanded), 1);
}
