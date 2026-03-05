namespace ScreenDrafts.Modules.Drafts.Features.People.Search;

internal sealed class SearchPeopleQueryHandler(IDbConnectionFactory dbConnectionFactory) : IQueryHandler<SearchPeopleQuery, PagedResult<SearchPeopleResponse>>
{
  private readonly IDbConnectionFactory _dbConnectionFactory = dbConnectionFactory;

  public async Task<Result<PagedResult<SearchPeopleResponse>>> Handle(SearchPeopleQuery request, CancellationToken cancellationToken)
  {
    await using var connection = await _dbConnectionFactory.OpenConnectionAsync(cancellationToken);

    const string baseSql =
      $"""
        SELECT
          p.public_id as {nameof(SearchPeopleResponse.PublicId)},
          p.first_name AS {nameof(SearchPeopleResponse.FirstName)},
          p.last_name AS {nameof(SearchPeopleResponse.LastName)},
          p.display_name as {nameof(SearchPeopleResponse.DisplayName)},
          (d.id IS NOT NULL) AS {nameof(SearchPeopleResponse.IsDrafter)},
          (h.id IS NOT NULL) AS {nameof(SearchPeopleResponse.IsHost)}
        FROM
          drafts.people p
        LEFT JOIN drafts.drafters d ON d.person_id = p.id
        LEFT JOIN drafts.hosts h ON h.person_id = p.id
        WHERE 1 = 1
      """;

    var sql = new StringBuilder(baseSql);
    var p = new DynamicParameters();

    if (!string.IsNullOrWhiteSpace(request.Name))
    {
      sql.Append(
        " AND (p.first_name ILIKE @name OR p.last_name ILIKE @name OR p.display_name ILIKE @name)");
      p.Add("name", $"%{request.Name}%");
    }

    if (!string.IsNullOrWhiteSpace(request.Role))
    {
      var normalizedRole = request.Role.Trim().ToUpperInvariant();
      if (normalizedRole == "DRAFTER")
      {
        sql.Append(" AND d.id IS NOT NULL");
      }
      else if (normalizedRole == "HOST")
      {
        sql.Append(" AND h.id IS NOT NULL");
      }
    }

    sql.Append(" ORDER BY p.last_name, p.first_name ASC");

    var totalCount = await connection.ExecuteScalarAsync<int>(
      new CommandDefinition(
      $"SELECT COUNT(*) FROM ({sql}) sub",
      parameters: p,
      cancellationToken: cancellationToken));

    var pageSize = Math.Min(request.PageSize, 100);
    var skip = (Math.Max(request.Page, 1) - 1) * pageSize;
    p.Add("pageSize", pageSize);
    p.Add("skip", skip);
    sql.Append(" LIMIT @pageSize OFFSET @skip");

    var items = (await connection.QueryAsync<SearchPeopleResponse>(
      new CommandDefinition(
      sql.ToString(),
      parameters: p,
      cancellationToken: cancellationToken))).ToList();

    return Result.Success(new PagedResult<SearchPeopleResponse>
    {
      Items = items,
      TotalCount = totalCount,
      Page = request.Page,
      PageSize = pageSize
    });
  }
}



