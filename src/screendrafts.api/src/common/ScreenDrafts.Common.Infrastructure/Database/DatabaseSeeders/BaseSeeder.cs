using ILogger = Microsoft.Extensions.Logging.ILogger;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace ScreenDrafts.Common.Infrastructure.Database.DatabaseSeeders;

[System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1002:Do not expose generic lists", Justification = "Reviewed")]
[System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1051:Do not declare visible instance fields", Justification = "Reviewed")]
public abstract class BaseSeeder(ILogger logger, ICsvFileService csvFileService)
{
  protected readonly ILogger _logger = logger;
  protected readonly ICsvFileService _csvFileService = csvFileService;

  protected static string ResolvePath(string fileName)
  {
    var dataPath = Environment.GetEnvironmentVariable("DATA_PATH")
      ?? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data");
    return Path.Combine(dataPath, fileName);
  }

  protected bool EnsureFileExists(string fileName, string tableName)
  {
    if (!File.Exists(fileName))
    {
      DatabaseSeedingLoggingMessages.FileNotFound(_logger, fileName);
      return false;
    }

    DatabaseSeedingLoggingMessages.StartingSeeding(_logger, tableName);
    return true;
  }

  protected (string FullPath, bool Exists) ResolveFile(SeedFile file, string tableName)
  {
    ArgumentNullException.ThrowIfNull(file);

    var fullPath = ResolvePath(file.FileName);
    var exists = EnsureFileExists(fullPath, tableName);
    return (fullPath, exists);
  }

  protected List<T> ReadCsv<T>(SeedFile seedFile, string tableName)
  {


    var (filePath, exists) = ResolveFile(seedFile, tableName);
    if (!exists)
    {
      return [];
    }

    var records = _csvFileService.ReadCsvFile<T>(filePath).ToList();
    return records ?? [];
  }

  protected async Task<string[]> ReadRawLinesAsync(SeedFile seedFile, string tableName, CancellationToken cancellationToken)
  {
    var (filePath, exists) = ResolveFile(seedFile, tableName);
    if (!exists)
    {
      return [];
    }

    var lines = await File.ReadAllLinesAsync(filePath, cancellationToken);
    return lines.Skip(1).ToArray() ?? [];
  }

  protected async Task<T?> ReadJsonAsync<T>(SeedFile seedFile, string tableName, CancellationToken cancellationToken)
  {
    var (filePath, exists) = ResolveFile(seedFile, tableName);
    if (!exists)
    {
      return default;
    }
    var json = await File.ReadAllTextAsync(filePath, cancellationToken);
    return JsonSerializer.Deserialize<T>(json, SerializerOptions.Instance);
  }
  protected void LogInsertComplete(string tableName, int count)
  {
    DatabaseSeedingLoggingMessages.BulkInsertComplete(_logger, count, tableName);
    DatabaseSeedingLoggingMessages.SeedingComplete(_logger, tableName);
  }
}
