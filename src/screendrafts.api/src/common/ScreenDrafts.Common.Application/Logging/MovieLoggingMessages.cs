namespace ScreenDrafts.Common.Application.Logging;

public static class MovieLoggingMessages
{
  private static readonly Action<ILogger, int, Exception?> _movieAlreadyExists = LoggerMessage.Define<int>(
    LogLevel.Warning,
    new EventId(1, nameof(MovieAlreadyExists)),
    "The movie with TMDB Id {TmdbId} already exists.");

  private static readonly Action<ILogger, string, Exception?> _movieAlreadyExistsWithImdbId = LoggerMessage.Define<string>(
    LogLevel.Warning,
    new EventId(1, nameof(MovieAlreadyExists)),
    "The movie with PublicId {PublicId} already exists.");

  private static readonly Action<ILogger, string, Exception?> _createMovieFailed = LoggerMessage.Define<string>(
    LogLevel.Error,
    new EventId(2, nameof(CreateMovieFailed)),
    "Movie.Create() failed: {Error}");

  private static readonly Action<ILogger, string, string, Exception?> _imdbMovieAddedToDatabase = LoggerMessage.Define<string, string>(
    LogLevel.Information,
    new EventId(3, nameof(MovieAddedToDatabase)),
    "Movie added to database: {Title} (IMDB Id: {ImdbId})");

  private static readonly Action<ILogger, string, int, Exception?> _tmdbMovieAddedToDatabase = LoggerMessage.Define<string, int>(
    LogLevel.Information,
    new EventId(3, nameof(MovieAddedToDatabase)),
    "Movie added to database: {Title} (TMDB Id: {TmdbId})");

  private static readonly Action<ILogger, int, Exception?> _fetchedMovieFromTmdb = LoggerMessage.Define<int>(
    LogLevel.Information,
    new EventId(4, nameof(FetchedMovieFromTmdb)),
    "Fetched movie with Tmdb Id {TmdbId}.");

  private static readonly Action<ILogger, int, Exception?> _imdbIdNotFound = LoggerMessage.Define<int>(
    LogLevel.Warning,
    new EventId(5, nameof(ImdbIdNotFound)),
    "IMDB Id not found for movie with TMDB Id {TmdbId}.");

  public static void MovieAlreadyExists(ILogger logger, string publicId) => _movieAlreadyExistsWithImdbId(logger, publicId, null);

  public static void MovieAlreadyExists(ILogger logger, int tmdbId) => _movieAlreadyExists(logger, tmdbId, null);

  public static void CreateMovieFailed(ILogger logger, string error) => _createMovieFailed(logger, error, null);

  public static void MovieAddedToDatabase(ILogger logger, string title, string imdbId) => _imdbMovieAddedToDatabase(logger, title, imdbId, null);

  public static void MovieAddedToDatabase(ILogger logger, string title, int tmdbId) => _tmdbMovieAddedToDatabase(logger, title, tmdbId, null);

  public static void FetchedMovieFromTmdb(ILogger logger, int tmdbId) => _fetchedMovieFromTmdb(logger, tmdbId, null);

  public static void ImdbIdNotFound(ILogger logger, int tmdbId) => _imdbIdNotFound(logger, tmdbId, null);
}
