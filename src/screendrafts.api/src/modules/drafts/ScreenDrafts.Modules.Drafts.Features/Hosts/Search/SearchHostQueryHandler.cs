namespace ScreenDrafts.Modules.Drafts.Features.Hosts.Search;

internal sealed class SearchHostQueryHandler(IDbConnectionFactory connectionFactory)
  : IQueryHandler<SearchHostQuery, PagedResult<SearchHostResponse>>
{
  private readonly IDbConnectionFactory _connectionFactory = connectionFactory;

  public async Task<Result<PagedResult<SearchHostResponse>>> Handle(SearchHostQuery request, CancellationToken cancellationToken)
  {
    await using var connection = await _connectionFactory.OpenConnectionAsync(cancellationToken);

    var primaryRoleValue = HostRole.Primary.Value;

    const string baseFrom =
      """
      FROM drafts.hosts h
      JOIN drafts.people p ON h.person_id = p.id
      """;

    var where = new StringBuilder("WHERE 1 = 1");
    var p = new DynamicParameters();

    if (!string.IsNullOrWhiteSpace(request.Name))
    {
      where.Append(" AND (p.first_name ILIKE @Name OR p.last_name ILIKE @Name OR p.display_name ILIKE @Name)");
      p.Add("Name", $"%{request.Name.Trim()}%");
    }

    if (request.HasBeenPrimary.HasValue)
    {
      var existsClause = request.HasBeenPrimary.Value
        ? ""
        : "NOT ";
      where.Append(
        CultureInfo.InvariantCulture,
        $" AND {existsClause}EXISTS (SELECT 1 FROM drafts.draft_hosts dh2 WHERE dh2.host_id = h.id AND dh2.role = @PrimaryRole)");

      p.Add("PrimaryRole", primaryRoleValue);
    }

    var countSql = $"SELECT COUNT(*) {baseFrom} {where}";

    var totalCount = await connection.ExecuteScalarAsync<int>(
      new CommandDefinition(
        countSql,
        p,
        cancellationToken: cancellationToken));

    if (totalCount == 0)
    {
      return Result.Success(new PagedResult<SearchHostResponse>
      {
        Items = [],
        TotalCount = 0,
        Page = request.Page,
        PageSize = request.PageSize
      });
    }

    var orderBy = request.SortBy?.ToLowerInvariant() switch
    {
      "hostedcount" => "hosted_draft_part_count DESC",
      _ => "p.last_name ASC, p.first_name ASC"
    };

    var pageSize = Math.Min(request.PageSize, 100);
    var offset = (Math.Max(request.Page, 1) - 1) * pageSize;
    p.Add("PageSize", pageSize);
    p.Add("Offset", offset);

    var pageSql =
      $"""
      SELECT
        h.public_id AS {nameof(SearchHostResponse.PublicId)},
        p.public_id AS {nameof(SearchHostResponse.PersonPublicId)}, 
        p.first_name AS {nameof(SearchHostResponse.FirstName)},
        p.last_name AS {nameof(SearchHostResponse.LastName)},
        p.display_name AS {nameof(SearchHostResponse.DisplayName)},
        COUNT(dh.draft_part_id) AS {nameof(SearchHostResponse.HostedDraftPartCount)}
      {baseFrom}
      LEFT JOIN drafts.draft_hosts dh ON dh.host_id = h.id
      {where}
      GROUP BY h.id, h.public_id, p.public_id, p.first_name, p.last_name, p.display_name
      ORDER BY {orderBy}, h.public_id ASC
      LIMIT @PageSize OFFSET @Offset
      """;

    var items = (await connection.QueryAsync<SearchHostResponse>(
      new CommandDefinition(
        pageSql,
        p,
        cancellationToken: cancellationToken)))
        .ToList();

    return Result.Success(new PagedResult<SearchHostResponse>
    {
      Items = items,
      TotalCount = totalCount,
      Page = request.Page,
      PageSize = request.PageSize
    });
  }
}
