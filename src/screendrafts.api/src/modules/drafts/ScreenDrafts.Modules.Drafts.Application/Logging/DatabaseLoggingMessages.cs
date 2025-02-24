namespace ScreenDrafts.Modules.Drafts.Application.Logging;

public static class DatabaseLoggingMessages
{
  private static readonly Action<ILogger, Exception?> _startingSeeding = LoggerMessage.Define(
    LogLevel.Information,
    new EventId(1, nameof(StartingSeeding)),
    "Starting database seeding...");

  private static readonly Action<ILogger, string, Exception?> _fileNotFound = LoggerMessage.Define<string>(
    LogLevel.Error,
    new EventId(2, nameof(FileNotFound)),
    "The file was not found: {FileName}");

  private static readonly Action<ILogger, Exception?> _seedingComplete = LoggerMessage.Define(
    LogLevel.Information,
    new EventId(3, nameof(SeedingComplete)),
    "Database seeding complete.");

  private static readonly Action<ILogger, int, string, Exception?> _bulkInsertMessage = LoggerMessage.Define<int, string>(
    LogLevel.Information,
    new EventId(4, nameof(BulkInsertMessage)),
    "Seeding {Count} drafters from {FilePath}.");

  private static readonly Action<ILogger, int, Exception?> _bulkInsertComplete = LoggerMessage.Define<int>(
    LogLevel.Information,
    new EventId(5, nameof(BulkInsertComplete)),
    "Inserted {Count} drafters successfully.");

  public static void StartingSeeding(ILogger logger) => _startingSeeding(logger, null);

  public static void FileNotFound(ILogger logger, string filePath) => _fileNotFound(logger, filePath, null);

  public static void SeedingComplete(ILogger logger) => _seedingComplete(logger, null);

  public static void BulkInsertMessage(ILogger logger, int count, string filePath) => _bulkInsertMessage(logger, count, filePath, null);

  public static void BulkInsertComplete(ILogger logger, int count) => _bulkInsertComplete(logger, count, null);
}
