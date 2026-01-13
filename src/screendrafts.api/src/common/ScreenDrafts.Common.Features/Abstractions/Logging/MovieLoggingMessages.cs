namespace ScreenDrafts.Common.Features.Abstractions.Logging;

public static class MovieLoggingMessages
{
  private static readonly Action<ILogger, string, Exception?> _movieAlreadyExists = LoggerMessage.Define<string>(
    LogLevel.Warning,
    new EventId(1, nameof(MovieAlreadyExists)),
    "The movie with IMDB Id {ImdbId} already exists.");

  private static readonly Action<ILogger, string, Exception?> _createMovieFailed = LoggerMessage.Define<string>(
    LogLevel.Error,
    new EventId(2, nameof(CreateMovieFailed)),
    "Movie.Create() failed: {Error}");

  private static readonly Action<ILogger, string, string, Exception?> _movieAddedToDatabase = LoggerMessage.Define<string, string>(
    LogLevel.Information,
    new EventId(3, nameof(MovieAddedToDatabase)),
    "Movie added to database: {Title} (IMDB Id: {ImdbId})");

  private static readonly Action<ILogger, string, Exception?> _fetchedMovieFromImdb = LoggerMessage.Define<string>(
    LogLevel.Information,
    new EventId(4, nameof(MovieAddedToDatabase)),
    "Fetched movie with Imdb Id {ImdbId}.");

  public static void MovieAlreadyExists(ILogger logger, string item) => _movieAlreadyExists(logger, item, null);

  public static void CreateMovieFailed(ILogger logger, string error) => _createMovieFailed(logger, error, null);

  public static void MovieAddedToDatabase(ILogger logger, string title, string imdbId) => _movieAddedToDatabase(logger, title, imdbId, null);

  public static void FetchedMovieFromImdb(ILogger logger, string imdbId) => _fetchedMovieFromImdb(logger, imdbId, null);
}
