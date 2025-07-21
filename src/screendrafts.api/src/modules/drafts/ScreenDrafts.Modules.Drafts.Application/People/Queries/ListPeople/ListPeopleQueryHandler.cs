namespace ScreenDrafts.Modules.Drafts.Application.People.Queries.ListPeople;

internal sealed class ListPeopleQueryHandler(IDbConnectionFactory dbConnectionFactory)
  : IQueryHandler<ListPeopleQuery, PagedResult<PersonResponse>>
{
  private readonly IDbConnectionFactory _dbConnectionFactory = dbConnectionFactory;

  private static readonly HashSet<string> _validSortColumns =
  [
    nameof(PersonResponse.FirstName),
    nameof(PersonResponse.LastName),
    nameof(PersonResponse.DisplayName)
  ];

  [System.Diagnostics.CodeAnalysis.SuppressMessage("Globalization", "CA1308:Normalize strings to uppercase", Justification = "Reviewed")]
  public async Task<Result<PagedResult<PersonResponse>>> Handle(ListPeopleQuery request, CancellationToken cancellationToken)
  {
    await using var connection = await _dbConnectionFactory.OpenConnectionAsync(cancellationToken);

    var sortColumn = _validSortColumns.Contains(request.Sort!) ? request.Sort
      : nameof(PersonResponse.LastName);

    var sortOrder = request.Dir?.ToLowerInvariant() == "desc" ? "DESC" : "ASC";

    var offset = (request.Page - 1) * request.PageSize;

    var whereClause =
      $"""
      WHERE
        (@Q IS NULL OR
         p.first_name ILIKE CONCAT('%', @Q, '%') OR
         p.last_name ILIKE CONCAT('%', @Q, '%') OR
         p.display_name ILIKE CONCAT('%', @Q, '%'))
      AND (@FirstName IS NULL OR p.first_name ILIKE @FirstName)
      AND (@LastName IS NULL OR p.last_name ILIKE @LastName)
      AND (@DisplayName IS NULL OR p.display_name ILIKE @DisplayName)
      AND (@IsDrafter IS NULL OR (@IsDrafter = TRUE AND d.id IS NOT NULL) OR (@IsDrafter = FALSE AND d.id IS NULL))
      AND (@IsHost IS NULL OR (@IsHost = TRUE AND h.id IS NOT NULL) OR (@IsHost = FALSE AND h.id IS NULL))
      """;

    var countSql =
      $"""
      SELECT COUNT(*)
      FROM drafts.people p
      LEFT JOIN drafts.drafters d ON d.person_id = p.id
      LEFT JOIN drafts.hosts h ON h.person_id = p.id
      {whereClause}
      """;

    var totalCount = await connection.ExecuteScalarAsync<int>(countSql, request);

    var baseSql =
      $"""
      SELECT
        p.id AS {nameof(PersonResponse.Id)},
        p.first_name AS {nameof(PersonResponse.FirstName)},
        p.last_name AS {nameof(PersonResponse.LastName)},
        p.display_name AS {nameof(PersonResponse.DisplayName)},
        CASE WHEN d.id IS NOT NULL THEN TRUE ELSE FALSE END AS {nameof(PersonResponse.IsDrafter)},
        CASE WHEN h.id IS NOT NULL THEN TRUE ELSE FALSE END AS {nameof(PersonResponse.IsHost)}
      FROM drafts.people p
      LEFT JOIN drafts.drafters d ON d.person_id = p.id
      LEFT JOIN drafts.hosts h ON h.person_id = p.id
      {whereClause}
      ORDER BY {sortColumn} {sortOrder}
      LIMIT @PageSize OFFSET @Offset
      """;

    var people = await connection.QueryAsync<PersonResponse>(baseSql, new
    {
      request.Q,
      request.FirstName,
      request.LastName,
      request.DisplayName,
      request.IsDrafter,
      request.IsHost,
      request.PageSize,
      Offset = offset
    });

    return new PagedResult<PersonResponse>(
      Items: [.. people],
      Total: totalCount,
      Page: request.Page,
      PageSize: request.PageSize);
  }
}
