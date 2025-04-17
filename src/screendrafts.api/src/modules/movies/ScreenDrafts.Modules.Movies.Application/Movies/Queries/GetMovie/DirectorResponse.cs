namespace ScreenDrafts.Modules.Movies.Application.Movies.Queries.GetMovie;

public sealed record DirectorResponse(
  Guid Id,
  string ImdbId,
  string Name)
{
  public DirectorResponse()
    : this(
        Guid.Empty,
        string.Empty,
        string.Empty)
  {
  }
}
