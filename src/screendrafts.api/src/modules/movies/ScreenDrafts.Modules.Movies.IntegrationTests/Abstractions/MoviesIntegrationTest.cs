using ScreenDrafts.Modules.Movies.IntegrationTests.TestUtils;

namespace ScreenDrafts.Modules.Movies.IntegrationTests.Abstractions;

[Collection(nameof(MoviesIntegrationTestCollection))]
public abstract class MoviesIntegrationTest(MoviesIntegrationTestWebAppFactory factory)
  : BaseIntegrationTest<MoviesDbContext>(factory)
{
  protected FakeIntegrationsApi FakeIntegrationsApi => factory.FakeIntegrationsApi;

  protected override async Task ClearDatabaseAsync()
  {
    await DbContext.Database.ExecuteSqlRawAsync(
      $"""
      TRUNCATE TABLE
        movies.genres,
        movies.media_actors,
        movies.media_directors,
        movies.media_genres,
        movies.media_producers,
        movies.media_production_companies,
        movies.media_writers,
        movies.media,
        movies.people,
        movies.production_companies
      RESTART IDENTITY CASCADE;
      """);
  }
}
