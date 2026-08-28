namespace ScreenDrafts.Modules.Movies.Features.Movies.GetMedia;

public sealed record ActorResponse(Guid Id, string ImdbId, int TmdbId, string Name)
{
  public ActorResponse()
    : this(Guid.Empty, string.Empty, 0, string.Empty) { }
}
