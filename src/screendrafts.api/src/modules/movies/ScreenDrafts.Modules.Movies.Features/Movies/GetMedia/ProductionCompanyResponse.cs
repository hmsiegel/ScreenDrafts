namespace ScreenDrafts.Modules.Movies.Features.Movies.GetMedia;

public sealed record ProductionCompanyResponse(Guid Id, string ImdbId, int TmdbId, string Name)
{
  public ProductionCompanyResponse()
    : this(Guid.Empty, string.Empty, 0, string.Empty) { }
}
