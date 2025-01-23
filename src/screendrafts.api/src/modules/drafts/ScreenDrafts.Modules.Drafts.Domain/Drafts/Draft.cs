namespace ScreenDrafts.Modules.Drafts.Domain.Drafts;

public sealed class Draft : Entity
{
  private Draft()
  {
  }

  public Guid Id { get; private set; }

  public string? Title { get; private set; }

  public DraftType? DraftType { get; private set; }

  public int NumberOfDrafters { get; private set; }

  public int NumberOfCommissioners { get; private set; }

  public int NumberOfMovies { get; private set; }

  public static Draft Create(
    string title,
    DraftType draftType,
    int numberOfDrafters,
    int numberOfCommissioners,
    int numberOfMovies)
  {
    return new Draft
    {
      Id = Guid.NewGuid(),
      Title = title,
      DraftType = draftType,
      NumberOfDrafters = numberOfDrafters,
      NumberOfCommissioners = numberOfCommissioners,
      NumberOfMovies = numberOfMovies
    };
  }
}
