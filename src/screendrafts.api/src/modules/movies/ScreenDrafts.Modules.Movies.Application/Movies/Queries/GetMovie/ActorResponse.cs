namespace ScreenDrafts.Modules.Movies.Application.Movies.Queries.GetMovie;

public sealed record ActorResponse(
  Guid Id,
  string ImdbId,
  string Name)
{
  public ActorResponse()
    : this(
        Guid.Empty,
        string.Empty,
        string.Empty)
  {
  }
}
