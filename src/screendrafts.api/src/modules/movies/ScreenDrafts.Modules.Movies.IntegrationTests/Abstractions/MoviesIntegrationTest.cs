namespace ScreenDrafts.Modules.Movies.IntegrationTests.Abstractions;

[Collection(nameof(MoviesIntegrationTestCollection))]
public abstract class MoviesIntegrationTest(MoviesIntegrationTestWebAppFactory factory)
  : BaseIntegrationTest<MoviesDbContext>(factory)
{
  protected override async Task ClearDatabaseAsync()
  {
    await DbContext.Database.ExecuteSqlRawAsync(
      $"""
      TRUNCATE TABLE 
        movies.genres,
        movies.movie_actors,
        movies.movie_directors,
        movies.movie_genres,
        movies.movie_producers,
        movies.movie_production_companies,
        movies.movie_writers,
        movies.movies,
        movies.people,
        movies.production_companies
      RESTART IDENTITY CASCADE;
      """);
  }
}
