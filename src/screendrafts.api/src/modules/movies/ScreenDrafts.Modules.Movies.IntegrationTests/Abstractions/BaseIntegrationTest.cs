namespace ScreenDrafts.Modules.Movies.IntegrationTests.Abstractions;

[Collection(nameof(IntegrationTestCollection))]
[System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1051:Do not declare visible instance fields", Justification = "Reviewed")]
public class BaseIntegrationTest : IDisposable, IAsyncLifetime
{
  private bool _disposedValue;
  protected static readonly Faker Faker = new();
  private readonly IServiceScope _serviceScope;
  protected readonly ISender Sender;
  protected readonly HttpClient HttpClient;
  protected readonly MoviesDbContext DbContext;

  public BaseIntegrationTest(IntegrationTestWebAppFactory factory)
  {
    ArgumentNullException.ThrowIfNull(factory);

    _serviceScope = factory.Services.CreateScope();
    Sender = _serviceScope.ServiceProvider.GetRequiredService<ISender>();
    HttpClient = factory.CreateClient();
    DbContext = _serviceScope.ServiceProvider.GetRequiredService<MoviesDbContext>();
  }

  protected virtual void Dispose(bool disposing)
  {
    if (!_disposedValue)
    {
      if (disposing)
      {
        _serviceScope.Dispose();
        HttpClient.Dispose();
        DbContext.Dispose();
      }

      _disposedValue = true;
    }
  }

  public async Task InitializeAsync()
  {
    await ClearDatabaseAsync();
  }

  public void Dispose()
  {
    Dispose(disposing: true);
    GC.SuppressFinalize(this);
  }

  public async Task DisposeAsync()
  {
    await ClearDatabaseAsync();
    Dispose();
  }

  private async Task ClearDatabaseAsync()
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
