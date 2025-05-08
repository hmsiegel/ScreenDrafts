using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace ScreenDrafts.Seeding.Movies.Common;

internal abstract class MovieBaseSeeder(
  MoviesDbContext dbContext,
  ILogger logger,
  ICsvFileService csvFileService) : BaseSeeder(logger, csvFileService)
{
  protected readonly MoviesDbContext _dbContext = dbContext;

  protected async Task InsertFromLinesAsync(
    string[] sourceData,
    string tableName,
    string[] columnNames,
    Func<string[], object[]> mapValues,
    SqlInsertHelper sqlInsertHelper,
    CancellationToken cancellationToken)
  {
    foreach (var line in sourceData)
    {
      var values = line.Split(',');

      if (values.Length < columnNames.Length)
      {
        continue;
      }

      var mappedValues = mapValues(values);

      await sqlInsertHelper.InsertIfNotExistsAsync(
        tableName,
        columnNames,
        mappedValues,
        cancellationToken);

      DatabaseSeedingLoggingMessages.ItemAddedToDatabase(_logger, string.Join(", ", mappedValues));
    }
  }

  protected async Task InsertIfNotExistsAsync<TSource, TEntity, TKey>(
    List<TSource> sourceData,
    Func<TSource, TKey> sourceKeySelector,
    Func<TEntity, TKey> entityKeySelector,
    Func<TSource, TEntity> createEntity,
    DbSet<TEntity> dbSet,
    string tableName,
    CancellationToken cancellationToken)
    where TEntity : class
  {
    var existingKeys = await dbSet
      .Select(entity => entityKeySelector(entity))
      .ToListAsync(cancellationToken);

    var existingKeySet = existingKeys.ToHashSet();

    var newEntities = sourceData
      .Where(source => !existingKeySet.Contains(sourceKeySelector(source)))
      .Select(createEntity)
      .ToList();

    if (newEntities.Count == 0)
    {
      DatabaseSeedingLoggingMessages.AlreadySeeded(_logger, tableName);
      return;
    }

    dbSet.AddRange(newEntities);
    await SaveAndLogAsync(tableName, newEntities.Count);
  }

  protected async Task SaveAndLogAsync(string tableName, int count)
  {
    await _dbContext.SaveChangesAsync();
    LogInsertComplete(tableName, count);
  }
}
