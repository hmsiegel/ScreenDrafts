namespace ScreenDrafts.Modules.Movies.Features.Movies.GetMedia;

public sealed record DirectorResponse(Guid Id, string ImdbId, int TmdbId, string Name)
{
  public DirectorResponse()
    : this(Guid.Empty, string.Empty, 0, string.Empty) { }
}
