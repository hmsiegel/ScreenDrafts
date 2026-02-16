namespace ScreenDrafts.Modules.Drafts.Features.People;

internal abstract class PaginatedDapperQueryHandler<TQuery, TResult>
  (IDbConnectionFactory dbConnectionFactory)
    where TQuery : class
{
  private readonly IDbConnectionFactory _dbConnectionFactory = dbConnectionFactory;

  /// <summary>
  /// Must return SQL SELECT COUNT(*) ... WHERE ... FROM [tableName] query
  /// </summary>
  protected abstract string GetCountSql();

  /// <summary>
  /// Must return SQL SELECT ... WHERE ... FROM [tableName] query
  /// Should include placeholders for {OrderByClause} and {PaginationClause}
  /// </summary>
  /// <returns></returns>
  protected abstract string GetDataSql();

  /// <summary>
  /// Allowes columns for sorting.
  /// </summary>
  /// <returns></returns>
  protected abstract IReadOnlySet<string> GetSortableColumns();

  [System.Diagnostics.CodeAnalysis.SuppressMessage("Globalization", "CA1308:Normalize strings to uppercase", Justification = "Reviewed")]
  public async Task<PagedResult<TResult>> HandleAsync(
    TQuery query,
    string? search,
    string sortColumn,
    string sortOrder,
    int pageNumber,
    int pageSize,
    CancellationToken cancellationToken = default)
  {
    await using var connection = await _dbConnectionFactory.OpenConnectionAsync(cancellationToken);

    var sortable = GetSortableColumns();
    var sortCol = sortable.Contains(sortColumn)
      ? sortColumn
      : sortable.FirstOrDefault();
    var sortDir = sortOrder?.ToLowerInvariant() == "desc" ? "DESC" : "ASC";
    var offset = (pageNumber - 1) * pageSize;

    var countSql = GetCountSql();
    var totalCount = await connection.ExecuteScalarAsync<int>(countSql, query);

    if (totalCount == 0)
    {
      return new PagedResult<TResult>
      {
        Items = [],
        TotalCount = 0,
        Page = pageNumber,
        PageSize = pageSize
      };
    }

    var dataSql = GetDataSql()
      .Replace(
      "{OrderByClause}",
      PeopleSqlQueryBuilder.BuildOrderClause(sortCol!, sortDir),
      StringComparison.InvariantCultureIgnoreCase)
      .Replace(
      "{PaginationClause}",
      PeopleSqlQueryBuilder.BuildPaginationClause,
      StringComparison.InvariantCultureIgnoreCase);

    var items = await connection.QueryAsync<TResult>(
      dataSql,
      new
      {
        Search = search,
        PageSize = pageSize,
        Offset = offset,
      });

    return new PagedResult<TResult>
    {
      Items = [..items],
      TotalCount = totalCount,
      Page = pageNumber,
      PageSize = pageSize
    };
  }
}
