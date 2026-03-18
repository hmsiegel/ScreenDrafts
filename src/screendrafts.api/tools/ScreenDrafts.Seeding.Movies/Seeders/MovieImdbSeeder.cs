using ScreenDrafts.Modules.Movies.Features.Movies.AddMovie;

using ValidationError = ScreenDrafts.Common.Abstractions.Errors.ValidationError;

namespace ScreenDrafts.Seeding.Movies.Seeders;

internal sealed partial class MovieImdbSeeder(
  MoviesDbContext dbContext,
  ILogger<MovieImdbSeeder> logger,
  ICsvFileService csvFileService,
  ISender sender)
  : MovieBaseSeeder(dbContext, logger, csvFileService), ICustomSeeder
{
  public int Order => 1;

  public string Name => "moviestmdb";

  private readonly ISender _sender = sender;

  public async Task InitializeAsync(CancellationToken cancellationToken = default)
  {
    await SeedMoviesImdbAsync(cancellationToken);
  }

  private async Task SeedMoviesImdbAsync(CancellationToken cancellationToken)
  {
    const string TableName = "Movies";

    var csvMovies = ReadCsv<MovieCsvModel>(
      new SeedFile(FileNames.MovieSeeder, SeedFileType.Csv),
      TableName);

    if (csvMovies.Count == 0)
    {
      return;
    }

    var movieIds = csvMovies.Select(movie => movie.TmdbId).ToList();

    var existingMovieIds = await _dbContext.Movies
      .Where(movie => movieIds.Contains(movie.TmdbId))
      .Select(movie => movie.TmdbId)
      .ToHashSetAsync(cancellationToken);

    var newMovies = csvMovies.Where(movie => !existingMovieIds.Contains(movie.TmdbId)).ToList();

    if (newMovies.Count == 0)
    {
      DatabaseSeedingLoggingMessages.AlreadySeeded(_logger, TableName);
      return;
    }

#pragma warning disable S3267 // Loop performs side effects and cannot be simplified
    foreach (var newMovie in newMovies)
    {
      try
      {
        var response = await _sender.Send(new GetOnlineMovieCommand(newMovie.TmdbId), cancellationToken);

        if (response is null || response.IsFailure)
        {
          Log_FailedToFetchMovie(newMovie.TmdbId);
          continue;
        }

        var actors = response.Value.Actors?.Select(actor => new PersonRequest(actor.Name, actor.ImdbId, 0)).ToList();
        var directors = response.Value.Directors?.Select(director => new PersonRequest(director.Name, director.ImdbId, 0)).ToList();
        var writers = response.Value.Writers?.Select(writer => new PersonRequest(writer.Name, writer.ImdbId, 0)).ToList();
        var producers = response.Value.Producers?.Select(producer => new PersonRequest(producer.Name, producer.ImdbId, 0)).ToList();
        var productionCompanies = response.Value.ProductionCompanies?
          .Select(company => new ProductionCompanyRequest(company.Name, company.ImdbId, 0))
          .ToList();
        var genres = response.Value.Genres
          .Select(g => new GenreRequest(g.Tmdb, g.Name))
          .ToList();

        var command = new AddMovieCommand(
          response.Value.ImdbId,
          response.Value.TmdbId,
          response.Value.Title,
          response.Value.Year,
          response.Value.Plot,
          response.Value.Image,
          response.Value.ReleaseDate,
          response.Value.TrailerUrl,
          genres,
          directors,
          actors,
          writers,
          producers,
          productionCompanies);

        var result = await _sender.Send(command, cancellationToken);

        if (result.IsFailure)
        {
          var errorMessages = result.Errors
            .SelectMany(e => e is ValidationError ve
              ? ve.Errors.Select(innerError => $"{innerError.Code}: {innerError.Description}")
              : [$"{e.Code}: {e.Description}"]);

          Log_FailedToAddMovie(response.Value.TmdbId, response.Value.Title, 
            string.Join(", ", errorMessages));
          continue;
        }
      }
      catch (ScreenDraftsException ex)
      {
        Log_ErrorFetchingMovie(ex, newMovie.TmdbId);

      }
    }
#pragma warning restore S3267
  }

  [LoggerMessage(Level = LogLevel.Warning, Message = "Failed to fetch movie details for TMDb ID: {tmdbId}. Skipping.")]
  private partial void Log_FailedToFetchMovie(int tmdbId);

  [LoggerMessage(Level = LogLevel.Error, Message = "An error occurred while fetching movie details for TMDb ID: {tmdbId}. Skipping.")]
  private partial void Log_ErrorFetchingMovie(Exception exception, int tmdbId);

  [LoggerMessage(Level = LogLevel.Error, Message = "Failed to add movie with TMDb ID: {tmdbId} - Title: {title}. Error: {errors}")]
  private partial void Log_FailedToAddMovie(int tmdbId, string title, string errors);
}
