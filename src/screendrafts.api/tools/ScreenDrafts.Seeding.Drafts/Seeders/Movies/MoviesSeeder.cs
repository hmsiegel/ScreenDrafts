namespace ScreenDrafts.Seeding.Drafts.Seeders.Movies;

internal sealed class MoviesSeeder(
  DraftsDbContext dbContext,
  ILogger<MoviesSeeder> logger,
  ICsvFileService csvFileService)
  : DraftBaseSeeder(dbContext, logger, csvFileService), ICustomSeeder
{
  public int Order => 11;

  public string Name => "movies";

  public async Task InitializeAsync(CancellationToken cancellationToken = default)
  {
    await SeedMoviesAsync(cancellationToken);
  }

  private async Task SeedMoviesAsync(CancellationToken cancellationToken)
  {
    const string TableName = "Movies";

    var csvMovies = ReadCsv<MoviesCsvModel>(
      new SeedFile(FileNames.MoviesSeeder, SeedFileType.Csv),
      TableName);

    if (csvMovies.Count == 0)
    {
      return;
    }

    var movieIds = csvMovies.Select(movie => movie.Id).ToList();
    var existingMovieIds = await _dbContext.Movies
      .Where(movie => movieIds.Contains(movie.Id))
      .Select(movie => movie.Id)
      .ToHashSetAsync(cancellationToken);

    var newMovies = csvMovies.Where(movie => !existingMovieIds.Contains(movie.Id)).ToList();

    if (newMovies.Count == 0)
    {
      DatabaseSeedingLoggingMessages.AlreadySeeded(_logger, TableName);
      return;
    }

    foreach (var movie in newMovies)
    {
      var newMovie = Movie.Create(
        movieTitle: movie.Title,
        imdbId: movie.ImdbId,
        id: movie.Id).Value;

      _dbContext.Movies.Add(newMovie);
      DatabaseSeedingLoggingMessages.ItemAddedToDatabase(_logger, newMovie.MovieTitle);
    }

    await SaveAndLogAsync(
      tableName: TableName,
      count: newMovies.Count);
  }
}
