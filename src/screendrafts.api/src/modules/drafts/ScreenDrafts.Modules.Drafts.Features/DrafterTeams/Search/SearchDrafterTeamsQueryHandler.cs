namespace ScreenDrafts.Modules.Drafts.Features.DrafterTeams.Search;

internal sealed class SearchDrafterTeamsQueryHandler(IDbConnectionFactory dbConnectionFactory)
  : IQueryHandler<SearchDrafterTeamsQuery, PagedResult<SearchDrafterTeamsResponse>>
{
  private readonly IDbConnectionFactory _dbConnectionFactory = dbConnectionFactory;

  public async Task<Result<PagedResult<SearchDrafterTeamsResponse>>> Handle(SearchDrafterTeamsQuery request, CancellationToken cancellationToken)
  {
    await using var connection = await _dbConnectionFactory.OpenConnectionAsync(cancellationToken);

    const string baseSql =
      $"""
      SELECT
        dt.public_id AS {nameof(SearchDrafterTeamsResponse.PublicId)},
        dt.name AS {nameof(SearchDrafterTeamsResponse.Name)},
        dt.number_of_drafters AS {nameof(SearchDrafterTeamsResponse.NumberOfDrafters)}
      FROM drafts.drafter_teams dt
      WHERE 1 = 1
      """;

    var sql = new StringBuilder(baseSql);
    var p = new DynamicParameters();

    if (!string.IsNullOrWhiteSpace(request.Name))
    {
      sql.Append(" AND dt.name ILIKE '%' || @name || '%'");
      p.Add("name", request.Name);
    }

    sql.Append(" ORDER BY dt.name ASC");

    var totalCount = await connection.ExecuteScalarAsync<int>(
      new CommandDefinition(
        $"SELECT COUNT(*) FROM ({sql}) AS count_query",
        p,
        cancellationToken: cancellationToken));

    var pageSize = Math.Min(request.PageSize, 100);
    var skip = (Math.Max(request.Page, 1) - 1) * pageSize;
    p.Add("pageSize", pageSize);
    p.Add("skip", skip);
    sql.Append(" LIMIT @pageSize OFFSET @skip");

    var items = (await connection.QueryAsync<SearchDrafterTeamsResponse>(
      new CommandDefinition(
        sql.ToString(),
        p,
        cancellationToken: cancellationToken))).ToList();

    return Result.Success(new PagedResult<SearchDrafterTeamsResponse>
    {
      Items = items,
      TotalCount = totalCount,
      Page = request.Page,
      PageSize = pageSize
    });
  }
}
