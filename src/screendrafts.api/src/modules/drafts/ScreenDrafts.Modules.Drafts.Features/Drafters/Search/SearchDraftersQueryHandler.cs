namespace ScreenDrafts.Modules.Drafts.Features.Drafters.Search;

internal sealed class SearchDraftersQueryHandler(IDbConnectionFactory dbConnectionFactory)
  : IQueryHandler<SearchDraftersQuery, PagedResult<SearchDraftersResponse>>
{
  private readonly IDbConnectionFactory _dbConnectionFactory = dbConnectionFactory;

  public async Task<Result<PagedResult<SearchDraftersResponse>>> Handle(
    SearchDraftersQuery request,
    CancellationToken cancellationToken
  )
  {
    await using var connection = await _dbConnectionFactory.OpenConnectionAsync(cancellationToken);

    const string baseFrom = """
      FROM drafts.drafters d
      JOIN drafts.people p ON p.id = d.person_id
      """;

    var where = new StringBuilder("WHERE 1 = 1");
    var p = new DynamicParameters();

    if (!string.IsNullOrWhiteSpace(request.Name))
    {
      where.Append(" AND p.display_name ILIKE @Name");
      p.Add("Name", $"%{request.Name.Trim()}%");
    }

    if (request.IsRetired.HasValue)
    {
      where.Append(" AND d.is_retired = @Retired");
      p.Add("Retired", request.IsRetired.Value);
    }

    var countSql = $"SELECT COUNT(*) {baseFrom} {where}";

    // S2077: countSql is our own app-built query text (constants + WHERE clause built from fixed fragments); all values are bound via Dapper parameters above.
#pragma warning disable S2077
    var totalCount = await connection.ExecuteScalarAsync<int>(
      new CommandDefinition(countSql, p, cancellationToken: cancellationToken)
    );
#pragma warning restore S2077

    if (totalCount == 0)
    {
      return Result.Success(
        new PagedResult<SearchDraftersResponse>
        {
          Items = [],
          TotalCount = 0,
          Page = request.Page,
          PageSize = request.PageSize,
        }
      );
    }

    var pageSize = Math.Min(request.PageSize, 500);
    var offset = (Math.Max(request.Page, 1) - 1) * pageSize;
    p.Add("PageSize", pageSize);
    p.Add("Offset", offset);

    var pageSql = $"""
      SELECT
        d.public_id  AS {nameof(SearchDraftersResponse.PublicId)},
        p.public_id  AS {nameof(SearchDraftersResponse.PersonPublicId)},
        p.display_name AS {nameof(SearchDraftersResponse.DisplayName)},
        d.is_retired AS {nameof(SearchDraftersResponse.IsRetired)}
      {baseFrom}
      {where}
      ORDER BY p.display_name ASC
      LIMIT @PageSize OFFSET @Offset
      """;

    // S2077: pageSql is our own app-built query text (constants + WHERE clause built from fixed fragments); all values are bound via Dapper parameters above.
#pragma warning disable S2077
    var items = (
      await connection.QueryAsync<SearchDraftersResponse>(
        new CommandDefinition(pageSql, p, cancellationToken: cancellationToken)
      )
    ).ToList();
#pragma warning restore S2077

    return Result.Success(
      new PagedResult<SearchDraftersResponse>
      {
        Items = items,
        TotalCount = totalCount,
        Page = request.Page,
        PageSize = request.PageSize,
      }
    );
  }
}
