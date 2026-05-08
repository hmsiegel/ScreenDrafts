namespace ScreenDrafts.Seeding.Honorifics.Common;

internal sealed class SqlInsertHelper(ReportingDbContext dbContext)
{
  private readonly ReportingDbContext _dbContext = dbContext;

  public async Task InsertIfNotExistsAsync(
    string table,
    string[] columns,
    object[] values,
    CancellationToken cancellationToken = default
  )
  {
    var columnList = string.Join(", ", columns);
    var paramList = string.Join(", ", columns.Select((_, i) => $"{{{i}}}"));

    var sql = $"""
      INSERT INTO {table} ({columnList})
      VALUES ({paramList})
      ON CONFLICT DO NOTHING
      """;

    await _dbContext.Database.ExecuteSqlRawAsync(sql, values, cancellationToken);
  }
}
