namespace ScreenDrafts.Modules.Movies.Features.Movies.GetMedia;

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
