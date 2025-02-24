namespace ScreenDrafts.Modules.Movies.Domain.Movies.Entities;

public sealed class MovieProductionCompany
{
  private MovieProductionCompany(
    MovieId movieId,
    Guid genreId)
  {
    MovieId = movieId;
    ProductionCompanyId = genreId;
  }

  private MovieProductionCompany()
  {
  }

  public MovieId MovieId { get; private set; } = default!;

  public Movie Movie { get; private set; } = default!;

  public Guid ProductionCompanyId { get; private set; } = Guid.Empty;

  public ProductionCompany ProductionCompany { get; private set; } = default!;

  public static MovieProductionCompany Create(
    MovieId movieId,
    Guid genreId)
  {
    return new MovieProductionCompany(movieId, genreId);
  }
}
