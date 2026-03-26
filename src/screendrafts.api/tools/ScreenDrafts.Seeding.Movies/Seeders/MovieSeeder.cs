namespace ScreenDrafts.Seeding.Movies.Seeders;

internal sealed partial class MovieSeeder(
  ILogger<MovieSeeder> logger,
  ICsvFileService csvFileService,
  MoviesDbContext dbContext,
  IPublicIdGenerator publicIdGenerator)
  : MovieBaseSeeder(dbContext, logger, csvFileService), ICustomSeeder
{
  public int Order => 2;

  public string Name => "movies";

  private readonly IPublicIdGenerator _publicIdGenerator = publicIdGenerator;

  public async Task InitializeAsync(CancellationToken cancellationToken = default)
  {
    await SeedMoviesExportAsync(cancellationToken);
  }

  private async Task SeedMoviesExportAsync(CancellationToken cancellationToken)
  {
    const string TableName = "Movies";

    var csvMovies = ReadCsv<MovieExportCsvModel>(
      new SeedFile(FileNames.MovieExportSeeder, SeedFileType.Csv),
      TableName)
      .Where(r => !string.IsNullOrWhiteSpace(r.Title))
      .ToList();

    if (csvMovies.Count == 0)
    {
      return;
    }

    var movieIds = csvMovies.Select(movie => movie.ImdbId).ToList();
    var existingMovieIds = await _dbContext.Media
      .Where(movie => movieIds.Contains(movie.ImdbId!))
      .Select(movie => movie.ImdbId)
      .ToHashSetAsync(cancellationToken);

    var newMovies = csvMovies.Where(movie => !existingMovieIds.Contains(movie.ImdbId)).ToList();

    if (newMovies.Count == 0)
    {
      DatabaseSeedingLoggingMessages.AlreadySeeded(_logger, TableName);
      return;
    }

    Log_FoundItemsToAdd(newMovies.Count);

    foreach (var newMovie in newMovies)
    {
      var uri = string.IsNullOrWhiteSpace(newMovie.YouTubeTrailerUrl)
        ? null
        : new Uri(newMovie.YouTubeTrailerUrl);

      var mediaId = string.IsNullOrWhiteSpace(newMovie.Id) 
        ? MediaId.CreateUnique()
        : MediaId.Create(Guid.Parse(newMovie.Id));


      var movieResult = Media.Create(
        publicId: _publicIdGenerator.GeneratePublicId(PublicIdPrefixes.Media),
        title: newMovie.Title,
        year: newMovie.Year,
        plot: newMovie.Plot,
        image: newMovie.Image,
        releaseDate: newMovie.ReleaseDate,
        youtubeTrailerUrl: uri,
        imdbId: newMovie.ImdbId,
        tmdbId: newMovie.TmdbId,
        igdbId: null,
        externalId: null,
        mediaType: MediaType.FromValue(newMovie.MediaType),
        id: mediaId);

      if (movieResult.IsFailure)
      {
        Log_FailedToAddItem(newMovie.Title, movieResult.Errors[0].Description.ToString());
        continue;
      }

      var movie = movieResult.Value;

      _dbContext.Media.Add(movie);

      Log_AddedItem(movie.Title);
    }

    await SaveAndLogAsync(TableName, newMovies.Count);
  }

  [LoggerMessage(
    EventId = 0,
    Level = LogLevel.Information,
    Message = "Added item {title} to the database.")]
  private partial void Log_AddedItem(string title);

  [LoggerMessage(
    EventId = 1,
    Level = LogLevel.Information,
    Message = "Found {count} items to add to the database.")]
  private partial void Log_FoundItemsToAdd(int count);

  [LoggerMessage(
    EventId = 2,
    Level = LogLevel.Error,
    Message = "Failed to add {item} to the database. Reason: {reason}")]
  private partial void Log_FailedToAddItem(string item, string reason);
}
