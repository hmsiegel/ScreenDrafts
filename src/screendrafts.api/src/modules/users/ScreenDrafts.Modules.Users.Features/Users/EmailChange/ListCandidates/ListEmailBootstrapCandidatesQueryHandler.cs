namespace ScreenDrafts.Modules.Users.Features.Users.EmailChange.ListCandidates;

internal sealed class ListEmailBootstrapCandidatesQueryHandler(
  IDbConnectionFactory dbConnectionFactory,
  IAdministrationApi administrationApi
) : IQueryHandler<ListEmailBootstrapCandidatesQuery, PagedResult<EmailBootstrapCandidateItem>>
{
  private readonly IDbConnectionFactory _dbConnectionFactory = dbConnectionFactory;
  private readonly IAdministrationApi _administrationApi = administrationApi;

  public async Task<Result<PagedResult<EmailBootstrapCandidateItem>>> Handle(
    ListEmailBootstrapCandidatesQuery request,
    CancellationToken cancellationToken
  )
  {
    var page = request.Page < 1 ? 1 : request.Page;
    var pageSize = request.PageSize < 1 ? 25 : request.PageSize;
    var searchPattern = string.IsNullOrWhiteSpace(request.Search)
      ? null
      : $"%{request.Search.Trim()}%";

    await using var connection = await _dbConnectionFactory.OpenConnectionAsync(cancellationToken);

    // "Needs migration" = no claim row yet, or a claim row that hasn't been
    // claimed. Anyone with claimed_at set has already completed the move to a
    // real email and is excluded from this list entirely.
    const string whereClause = """
      WHERE c.claimed_at IS NULL
        AND u.is_social_login = FALSE
        AND (@SearchPattern IS NULL
          OR u.email ILIKE @SearchPattern
          OR u.first_name ILIKE @SearchPattern
          OR u.last_name ILIKE @SearchPattern)
      """;

    var countSql = $"""
      SELECT COUNT(*)
      FROM users.users u
      LEFT JOIN users.email_bootstrap_claims c ON c.user_id = u.id
      {whereClause};
      """;

    var pageSql = $"""
      SELECT
        u.public_id AS {nameof(CandidateRow.UserPublicId)},
        u.first_name AS {nameof(CandidateRow.FirstName)},
        u.last_name AS {nameof(CandidateRow.LastName)},
        u.email AS {nameof(CandidateRow.CurrentEmail)},
        (c.user_id IS NOT NULL) AS {nameof(CandidateRow.HasActiveToken)},
        c.expires_at AS {nameof(CandidateRow.TokenExpiresAt)}
      FROM users.users u
      LEFT JOIN users.email_bootstrap_claims c ON c.user_id = u.id
      {whereClause}
      ORDER BY u.last_name, u.first_name
      OFFSET @Offset LIMIT @PageSize;
      """;

    var parameters = new
    {
      SearchPattern = searchPattern,
      Offset = (page - 1) * pageSize,
      PageSize = pageSize,
    };

    var totalCount = await connection.ExecuteScalarAsync<int>(
      new CommandDefinition(countSql, parameters, cancellationToken: cancellationToken)
    );

    var rows = (
      await connection.QueryAsync<CandidateRow>(
        new CommandDefinition(pageSql, parameters, cancellationToken: cancellationToken)
      )
    ).ToList();

    var items = new List<EmailBootstrapCandidateItem>(rows.Count);

    foreach (var row in rows)
    {
      var roles = await _administrationApi.GetUserRolesAsync(row.UserPublicId, cancellationToken);
      var isPatreon = roles.Contains("Patreon", StringComparer.OrdinalIgnoreCase);

      items.Add(
        new EmailBootstrapCandidateItem
        {
          UserPublicId = row.UserPublicId,
          FirstName = row.FirstName,
          LastName = row.LastName,
          CurrentEmail = row.CurrentEmail,
          IsPatreon = isPatreon,
          HasActiveToken = row.HasActiveToken,
          TokenExpiresAt = row.TokenExpiresAt,
        }
      );
    }

    return new PagedResult<EmailBootstrapCandidateItem>
    {
      Items = items,
      TotalCount = totalCount,
      Page = page,
      PageSize = pageSize,
    };
  }

  private sealed record CandidateRow
  {
    public string UserPublicId { get; init; } = default!;
    public string FirstName { get; init; } = default!;
    public string LastName { get; init; } = default!;
    public string CurrentEmail { get; init; } = default!;
    public bool HasActiveToken { get; init; } = default!;
    public DateTimeOffset? TokenExpiresAt { get; init; } = default!;
  };
}
