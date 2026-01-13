namespace ScreenDrafts.Common.Features.Abstractions.Logging;

public static class DatabaseSeedingLoggingMessages
{
  private static readonly Action<ILogger, string, Exception?> _startingSeeding = LoggerMessage.Define<string>(
    LogLevel.Information,
    new EventId(1, nameof(StartingSeeding)),
    "Starting seeding the {TableName}...");

  private static readonly Action<ILogger, Exception?> _startingSeedingProcess = LoggerMessage.Define(
    LogLevel.Information,
    new EventId(2, nameof(StartingSeedingProcess)),
    "Starting the seeding process...");

  private static readonly Action<ILogger, Exception?> _seedingProcessComplete = LoggerMessage.Define(
    LogLevel.Information,
    new EventId(3, nameof(SeedingProcessComplete)),
    "Seeding process complete...");

  private static readonly Action<ILogger, string, Exception?> _fileNotFound = LoggerMessage.Define<string>(
    LogLevel.Error,
    new EventId(4, nameof(FileNotFound)),
    "The file was not found: {FileName}");

  private static readonly Action<ILogger, string, Exception?> _seedingComplete = LoggerMessage.Define<string>(
    LogLevel.Information,
    new EventId(5, nameof(SeedingComplete)),
    "{TableName} seeding complete.");

  private static readonly Action<ILogger, int, string, string, Exception?> _bulkInsertMessage = LoggerMessage.Define<int, string, string>(
    LogLevel.Information,
    new EventId(6, nameof(BulkInsertMessage)),
    "Seeding {Count} {TableName} from {FilePath}.");

  private static readonly Action<ILogger, int, string, Exception?> _bulkInsertComplete = LoggerMessage.Define<int, string>(
    LogLevel.Information,
    new EventId(7, nameof(BulkInsertComplete)),
    "Inserted {Count} rows into {TableName} successfully.");

  private static readonly Action<ILogger, string, string, Exception?> _alreadyExists = LoggerMessage.Define<string, string>(
    LogLevel.Information,
    new EventId(7, nameof(AlreadyExists)),
    "The item with Id {Id} already exists in the table {TableName}. Skipping.");

  private static readonly Action<ILogger, string, Exception?> _tablesEmpty = LoggerMessage.Define<string>(
    LogLevel.Information,
    new EventId(8, nameof(TablesEmpty)),
    "{TableName} is not empty. Skipping seeding.");

  private static readonly Action<ILogger, string, Exception?> _alreadySeeded = LoggerMessage.Define<string>(
    LogLevel.Information,
    new EventId(9, nameof(AlreadySeeded)),
    "{TableName} has already been seeded. Skipping.");

  private static readonly Action<ILogger, string, Exception?> _itemAddedToDatabase = LoggerMessage.Define<string>(
    LogLevel.Information,
    new EventId(10, nameof(ItemAddedToDatabase)),
    "Added {Item} to the database.");

  private static readonly Action<ILogger, string, Exception?> _unableToResolve = LoggerMessage.Define<string>(
    LogLevel.Error,
    new EventId(11, nameof(UnableToResolve)),
    "Skipped seeding: unable to resolve dependencies for: {Record}.");

  private static readonly Action<ILogger, string, string, string, Exception?> _recordMissing = LoggerMessage.Define<string, string, string>(
    LogLevel.Error,
    new EventId(12, nameof(RecordMissing)),
    "{Entity} record missing {Item} and resolution failed: {Record}");

  private static readonly Action<ILogger, string, string, string, Exception?> _notFound = LoggerMessage.Define<string, string, string>(
    LogLevel.Error,
    new EventId(13, nameof(NotFound)),
    "{Item} not found for {Entity} record: {Record}");

  public static readonly Action<ILogger, string, Exception?> _runningSeeder = LoggerMessage.Define<string>(
    LogLevel.Information,
    new EventId(14, nameof(RunningSeeder)),
    "Running seeder: {Seeder}");

  private static readonly Action<ILogger, int, int, int, int, Exception?> _batchProcessed = LoggerMessage.Define<int, int, int, int>(
    LogLevel.Information,
    new EventId(15, nameof(BatchProcessed)),
    "Processed batch {CurrentBatch}/{TotalBatches} ({ProcessedUsers}/{TotalUsers} users)");

  private static readonly Action<ILogger, string, Exception?> _adminUrl = LoggerMessage.Define<string>(
    LogLevel.Information,
    new EventId(16, nameof(AdminUrl)),
    "Admin URL: {Url}");

  public static void StartingSeeding(ILogger logger, string tableName) => _startingSeeding(logger, tableName, null);

  public static void StartingSeedingProcess(ILogger logger) => _startingSeedingProcess(logger, null);

  public static void SeedingProcessComplete(ILogger logger) => _seedingProcessComplete(logger, null);

  public static void FileNotFound(ILogger logger, string filePath) => _fileNotFound(logger, filePath, null);

  public static void SeedingComplete(ILogger logger, string tableName) => _seedingComplete(logger, tableName, null);

  public static void BulkInsertMessage(ILogger logger, int count, string filePath, string tableName) => _bulkInsertMessage(logger, count, filePath, tableName, null);

  public static void BulkInsertComplete(ILogger logger, int count, string tableName) => _bulkInsertComplete(logger, count, tableName, null);

  public static void AlreadyExists(ILogger logger, string item, string tableName) => _alreadyExists(logger, item, tableName, null);

  public static void TablesEmpty(ILogger logger, string tableName) => _tablesEmpty(logger, tableName, null);

  public static void AlreadySeeded(ILogger logger, string tableName) => _alreadySeeded(logger, tableName, null);

  public static void ItemAddedToDatabase(ILogger logger, string item) => _itemAddedToDatabase(logger, item, null);

  public static void UnableToResolve(ILogger logger, string record) => _unableToResolve(logger, record, null);

  public static void RecordMissing(ILogger logger, string entity, string item, string record) => _recordMissing(logger, entity, item, record, null);

  public static void NotFound(ILogger logger, string item, string blessing, string record) => _notFound(logger, item, blessing, record, null);

  public static void RunningSeeder(ILogger logger, string seeder) => _runningSeeder(logger, seeder, null);

  public static void BatchProcessed(ILogger logger, int currentBatch, int totalBatches, int processedUsers, int totalUsers) => _batchProcessed(logger, currentBatch, totalBatches, processedUsers, totalUsers, null);

  public static void AdminUrl(ILogger logger, string admin) => _adminUrl(logger, admin, null);
}
