namespace ScreenDrafts.Seeding.Movies.Common;

internal sealed class SqlInsertHelper(MoviesDbContext dbContext)
{
  private readonly MoviesDbContext _dbContext = dbContext;

  public async Task InsertIfNotExistsAsync(string table, string[] columns, object[] values, CancellationToken cancellationToken = default)
  {
    var columnList = string.Join(", ", columns);
    var paramList = string.Join(", ", columns.Select((_, i) => $"{{{i}}}"));

    var sql =
      $"""
      INSERT INTO {table} ({columnList})
      VALUES ({paramList})
      ON CONFLICT DO NOTHING
      """;

    // S2077: table/columnList are identifiers (table/column names), which can't be bound as SQL parameters.
    // This is an offline seeding CLI tool called only with hardcoded table/column names from seeder classes, not external input.
#pragma warning disable S2077
    await _dbContext.Database.ExecuteSqlRawAsync(
      sql,
      values,
      cancellationToken);
#pragma warning restore S2077
  }
}
