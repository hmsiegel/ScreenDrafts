namespace ScreenDrafts.Modules.Movies.Features.Movies.GetMedia;

public sealed record ProductionCompanyResponse(
  Guid Id,
  string ImdbId,
  string Name)
{
  public ProductionCompanyResponse()
    : this(Guid.Empty, string.Empty, string.Empty)
  {
  }
}
