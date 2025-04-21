namespace ScreenDrafts.Seeding.Movies.Seeders;

internal sealed class MovieSeeder(
  ILogger<MovieSeeder> logger,
  ICsvFileService csvFileService,
  MoviesDbContext dbContext)
  : MovieBaseSeeder(dbContext, logger, csvFileService), ICustomSeeder
{
  public int Order => 2;

  public string Name => "movies";

  public async Task InitializeAsync(CancellationToken cancellationToken = default)
  {
    await SeedMoviesExportAsync(cancellationToken);
  }

  private async Task SeedMoviesExportAsync(CancellationToken cancellationToken)
  {
    const string TableName = "Movies";

    var csvMovies = ReadCsv<MovieExportCsvModel>(
      new SeedFile(FileNames.MovieExportSeeder, SeedFileType.Csv),
      TableName);

    if (csvMovies.Count == 0)
    {
      return;
    }

    var movieIds = csvMovies.Select(movie => movie.ImdbId).ToList();
    var existingMovieIds = await _dbContext.Movies
      .Where(movie => movieIds.Contains(movie.ImdbId))
      .Select(movie => movie.ImdbId)
      .ToHashSetAsync(cancellationToken);

    var newMovies = csvMovies.Where(movie => !existingMovieIds.Contains(movie.ImdbId)).ToList();

    if (newMovies.Count == 0)
    {
      DatabaseSeedingLoggingMessages.AlreadySeeded(_logger, TableName);
      return;
    }

    foreach (var newMovie in newMovies)
    {
      var uri = string.IsNullOrWhiteSpace(newMovie.YouTubeTrailerUrl)
        ? null
        : new Uri(newMovie.YouTubeTrailerUrl);

      var movie = Movie.Create(
        newMovie.Title,
        newMovie.Year,
        newMovie.Plot,
        newMovie.Image,
        newMovie.ReleaseDate,
        uri,
        newMovie.ImdbId,
        MovieId.Create(Guid.Parse(newMovie.Id))).Value;
      _dbContext.Movies.Add(movie);
    }

    await SaveAndLogAsync(TableName, newMovies.Count);
  }
}
