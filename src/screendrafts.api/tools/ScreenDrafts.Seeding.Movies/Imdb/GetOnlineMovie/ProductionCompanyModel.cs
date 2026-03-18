namespace ScreenDrafts.Seeding.Movies.Imdb.GetOnlineMovie;

public class ProductionCompanyModel(string imdbId, string name, int tmdbId)
{
  public string ImdbId { get; init; } = imdbId;
  public int TmdbId { get; init; } = tmdbId;
  public string Name { get; init; } = name;
}
