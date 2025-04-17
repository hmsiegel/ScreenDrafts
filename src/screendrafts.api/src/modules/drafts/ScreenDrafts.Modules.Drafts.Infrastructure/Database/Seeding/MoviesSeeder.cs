namespace ScreenDrafts.Modules.Drafts.Infrastructure.Database.Seeding;

internal sealed class MoviesSeeder(
  DraftsDbContext dbContext,
  ILogger<MoviesSeeder> logger,
  ICsvFileService csvFileService) : ICustomSeeder
{
  private readonly DraftsDbContext _dbContext = dbContext;
  private readonly ILogger<MoviesSeeder> _logger = logger;
  private readonly ICsvFileService _csvFileService = csvFileService;

  public async Task InitializeAsync(CancellationToken cancellationToken = default)
  {
    var dataPath = Environment.GetEnvironmentVariable("DATA_PATH")
      ?? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data");
    var filePath = Path.Combine(dataPath, FileNames.MoviesSeeder);

    await SeedMoviesAsync(filePath, cancellationToken);
  }

  private async Task SeedMoviesAsync(string filePath, CancellationToken cancellationToken)
  {
    const string TableName = "Movies";

    if (!File.Exists(filePath))
    {
      DatabaseSeedingLoggingMessages.FileNotFound(_logger, filePath);
      return;
    }

    var csvMovies = _csvFileService.ReadCsvFile<MoviesCsvModel>(filePath)
      .ToList();

    if (csvMovies is null)
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

    await _dbContext.SaveChangesAsync(cancellationToken);
    DatabaseSeedingLoggingMessages.BulkInsertMessage(_logger, newMovies.Count, filePath, TableName);
    DatabaseSeedingLoggingMessages.SeedingComplete(_logger, TableName);
  }
}

public sealed class MoviesCsvModel
{
  [Column("id")]
  public Guid Id { get; set; }

  [Column("movie_title")]
  public string Title { get; set; } = default!;

  [Column("imdb_id")]
  public string ImdbId { get; set; } = default!;
}
