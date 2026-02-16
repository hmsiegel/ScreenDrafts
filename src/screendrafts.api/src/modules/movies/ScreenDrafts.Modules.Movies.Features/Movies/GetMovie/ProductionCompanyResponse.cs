namespace ScreenDrafts.Modules.Movies.Features.Movies.GetMovie;

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
