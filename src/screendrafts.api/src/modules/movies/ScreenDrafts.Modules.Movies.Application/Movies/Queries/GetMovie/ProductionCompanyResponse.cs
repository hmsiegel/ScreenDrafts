namespace ScreenDrafts.Modules.Movies.Application.Movies.Queries.GetMovie;

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
