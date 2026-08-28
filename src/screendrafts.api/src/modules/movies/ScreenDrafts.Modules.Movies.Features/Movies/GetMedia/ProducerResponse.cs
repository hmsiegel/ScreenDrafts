namespace ScreenDrafts.Modules.Movies.Features.Movies.GetMedia;

public sealed record ProducerResponse(Guid Id, string ImdbId, int TmdbId, string Name)
{
  public ProducerResponse()
    : this(Guid.Empty, string.Empty, 0, string.Empty) { }
}
