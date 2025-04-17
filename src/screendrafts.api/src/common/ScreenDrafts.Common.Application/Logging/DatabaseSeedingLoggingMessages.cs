namespace ScreenDrafts.Common.Application.Logging;

public static class DatabaseSeedingLoggingMessages
{
  private static readonly Action<ILogger, string, Exception?> _startingSeeding = LoggerMessage.Define<string>(
    LogLevel.Information,
    new EventId(1, nameof(StartingSeeding)),
    "Starting seeding the {TableName}...");

  private static readonly Action<ILogger, string, Exception?> _fileNotFound = LoggerMessage.Define<string>(
    LogLevel.Error,
    new EventId(2, nameof(FileNotFound)),
    "The file was not found: {FileName}");

  private static readonly Action<ILogger, string, Exception?> _seedingComplete = LoggerMessage.Define<string>(
    LogLevel.Information,
    new EventId(3, nameof(SeedingComplete)),
    "{TableName} seeding complete.");

  private static readonly Action<ILogger, int, string, string, Exception?> _bulkInsertMessage = LoggerMessage.Define<int, string, string>(
    LogLevel.Information,
    new EventId(4, nameof(BulkInsertMessage)),
    "Seeding {Count} {TableName} from {FilePath}.");

  private static readonly Action<ILogger, int, string, Exception?> _bulkInsertComplete = LoggerMessage.Define<int, string>(
    LogLevel.Information,
    new EventId(5, nameof(BulkInsertComplete)),
    "Inserted {Count} rows into {TableName} successfully.");

  private static readonly Action<ILogger, string, string, Exception?> _alreadyExists = LoggerMessage.Define<string, string>(
    LogLevel.Information,
    new EventId(6, nameof(AlreadyExists)),
    "The item with Id {Id} already exists in the table {TableName}. Skipping.");

  private static readonly Action<ILogger, string, Exception?> _tablesEmpty = LoggerMessage.Define<string>(
    LogLevel.Information,
    new EventId(7, nameof(TablesEmpty)),
    "{TableName} is not empty. Skipping seeding.");

  private static readonly Action<ILogger, string, Exception?> _alreadySeeded = LoggerMessage.Define<string>(
    LogLevel.Information,
    new EventId(8, nameof(AlreadySeeded)),
    "{TableName} has already been seeded. Skipping.");

  private static readonly Action<ILogger, string, Exception?> _itemAddedToDatabase = LoggerMessage.Define<string>(
    LogLevel.Information,
    new EventId(9, nameof(ItemAddedToDatabase)),
    "Added {Item} to the database.");

  private static readonly Action<ILogger, string, string, string, Exception?> _unableToResolve = LoggerMessage.Define<string, string, string>(
    LogLevel.Error,
    new EventId(10, nameof(UnableToResolve)),
    "Unable to resolve {Item} for {Blessing}: {Record}.");

  private static readonly Action<ILogger, string, string, string, Exception?> _recordMissing = LoggerMessage.Define<string, string, string>(
    LogLevel.Error,
    new EventId(11, nameof(RecordMissing)),
    "{Blessing} record missing {Item} and resolution failed: {Record}");

  private static readonly Action<ILogger, string, string, string, Exception?> _notFound = LoggerMessage.Define<string, string, string>(
    LogLevel.Error,
    new EventId(12, nameof(NotFound)),
    "{Item} not found for {Blessing} record: {Record}");

  public static void StartingSeeding(ILogger logger, string tableName) => _startingSeeding(logger, tableName, null);

  public static void FileNotFound(ILogger logger, string filePath) => _fileNotFound(logger, filePath, null);

  public static void SeedingComplete(ILogger logger, string tableName) => _seedingComplete(logger, tableName, null);

  public static void BulkInsertMessage(ILogger logger, int count, string filePath, string tableName) => _bulkInsertMessage(logger, count, filePath, tableName, null);

  public static void BulkInsertComplete(ILogger logger, int count, string tableName) => _bulkInsertComplete(logger, count, tableName, null);

  public static void AlreadyExists(ILogger logger, string item, string tableName) => _alreadyExists(logger, item, tableName, null);

  public static void TablesEmpty(ILogger logger, string tableName) => _tablesEmpty(logger, tableName, null);

  public static void AlreadySeeded(ILogger logger, string tableName) => _alreadySeeded(logger, tableName, null);

  public static void ItemAddedToDatabase(ILogger logger, string item) => _itemAddedToDatabase(logger, item, null);

  public static void UnableToResolve(ILogger logger, string item, string blessing, string record) => _unableToResolve(logger, item, blessing, record, null);

  public static void RecordMissing(ILogger logger, string blessing, string item, string record) => _recordMissing(logger, blessing, item, record, null);

  public static void NotFound(ILogger logger, string item, string blessing, string record) => _notFound(logger, item, blessing, record, null);
}
