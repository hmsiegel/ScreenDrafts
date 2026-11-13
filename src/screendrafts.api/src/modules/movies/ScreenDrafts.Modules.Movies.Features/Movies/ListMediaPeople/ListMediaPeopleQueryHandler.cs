namespace ScreenDrafts.Modules.Movies.Features.Movies.ListMediaPeople;

internal sealed class ListMediaPeopleQueryHandler(
  IDbConnectionFactory dbConnectionFactory,
  ICacheService cacheService
) : IQueryHandler<ListMediaPeopleQuery, ListMediaPeopleResponse>
{
  private const int DefaultPageSize = 48;
  private const int MaxPageSize = 100;

  // The unfiltered browse aggregates every credit row, so its pages are cached. Searches are
  // not: they match few people and the key space is unbounded.
  private static readonly TimeSpan BrowseTtl = TimeSpan.FromMinutes(10);

  private readonly IDbConnectionFactory _dbConnectionFactory = dbConnectionFactory;
  private readonly ICacheService _cacheService = cacheService;

  public async Task<Result<ListMediaPeopleResponse>> Handle(
    ListMediaPeopleQuery request,
    CancellationToken cancellationToken
  )
  {
    if (!MediaCreditRoles.TryResolve(request.Role, out var sources))
    {
      return Result.Failure<ListMediaPeopleResponse>(MediaErrors.InvalidCreditRole);
    }

    var page = request.Page < 1 ? 1 : request.Page;
    var pageSize = request.PageSize is < 1 or > MaxPageSize ? DefaultPageSize : request.PageSize;
    var term = request.Search?.Trim() ?? string.Empty;
    var order = BuildOrder(request.Sort);

    var cacheKey =
      term.Length == 0
        ? $"media-people-list:{request.Role?.Trim().ToLowerInvariant() ?? "any"}:{order.Key}:{page}:{pageSize}"
        : null;

    if (cacheKey is not null)
    {
      var cached = await _cacheService.GetAsync<CachedPeoplePage>(cacheKey, cancellationToken);
      if (cached is not null)
      {
        return Result.Success(ToResponse(cached.Items, cached.TotalCount, page, pageSize));
      }
    }

    await using var connection = await _dbConnectionFactory.OpenConnectionAsync(cancellationToken);

    // A person's key is their TMDb ID when they have one, otherwise their IMDb ID (many people
    // predate the TMDb import and have tmdb_id = 0). People with neither are skipped, as on the
    // person page. Grouping by key folds duplicate rows for one human into one entry.
    // Table and column names come from MediaCreditRoles' fixed list; the search term and paging
    // values are bound parameters.
    var credits = string.Join(
      " UNION ALL ",
      sources.Select(s =>
        $"SELECT '{RoleName(s)}'::text AS role, c.media_id, c.{s.PersonColumn} AS person_id FROM {s.Table} c JOIN matched mp ON mp.id = c.{s.PersonColumn}"
      )
    );

    var searchClause = term.Length == 0 ? string.Empty : "AND name ILIKE @Search";

    var cte = $"""
      WITH matched AS (
        SELECT
            id,
            name,
            CASE WHEN tmdb_id > 0 THEN 't:' || tmdb_id::text ELSE 'i:' || imdb_id END AS person_key
        FROM movies.people
        WHERE (tmdb_id > 0 OR COALESCE(imdb_id, '') <> '')
          {searchClause}
      ),
      credits AS (
        {credits}
      ),
      grouped AS (
        SELECT
          mt.person_key                                   AS person_key,
          MIN(mt.name)                                    AS name,
          LOWER(REGEXP_REPLACE(
            REGEXP_REPLACE(
              REGEXP_REPLACE(MIN(mt.name), '^\s+|\s+$', '', 'g'),
              '\s+(Jr\.?|Sr\.?|II|III|IV)$', '', 'i'),
            '^.*\s', ''))                                AS last_name,
          COUNT(DISTINCT cr.media_id)::int                AS title_count,
            ARRAY_AGG(DISTINCT cr.role ORDER BY cr.role)    AS roles
        FROM matched mt
        JOIN credits cr ON cr.person_id = mt.id
        GROUP BY mt.person_key
      )
      """;

    var pageSql = $"""
      {cte}
      SELECT
          person_key                 AS PersonKey,
          name                       AS Name,
          title_count                AS TitleCount,
          roles                      AS Roles,
          COUNT(*) OVER()::int       AS TotalCount
      FROM grouped
      {order.Clause}
      LIMIT @PageSize OFFSET @Offset
      """;

    var parameters = new DynamicParameters();
    parameters.Add("PageSize", pageSize);
    parameters.Add("Offset", (page - 1) * pageSize);
    if (term.Length > 0)
    {
      parameters.Add("Search", $"%{EscapeLike(term)}%");
    }

    // S2077: the SQL interpolates only fixed fragments (credit tables, a whitelisted ORDER BY); all values are bound.
#pragma warning disable S2077
    var rows = (await connection.QueryAsync<PersonRow>(pageSql, parameters)).ToList();

    var totalCount = rows.Count > 0 ? rows[0].TotalCount : 0;

    // A page past the end returns no rows, so the window count is missing. Count separately.
    if (rows.Count == 0 && page > 1)
    {
      totalCount = await connection.ExecuteScalarAsync<int>(
        $"{cte} SELECT COUNT(*)::int FROM grouped",
        parameters
      );
    }
#pragma warning restore S2077

    var items = rows.Select(r => new MediaPersonListItemResponse
      {
        Id = r.PersonKey[2..],
        Name = r.Name,
        TitleCount = r.TitleCount,
        Roles = r.Roles,
      })
      .ToList();

    if (cacheKey is not null)
    {
      await _cacheService.SetAsync(
        cacheKey,
        new CachedPeoplePage(items, totalCount),
        BrowseTtl,
        cancellationToken
      );
    }

    return Result.Success(ToResponse(items, totalCount, page, pageSize));
  }

  private static ListMediaPeopleResponse ToResponse(
    IReadOnlyList<MediaPersonListItemResponse> items,
    int totalCount,
    int page,
    int pageSize
  ) =>
    new()
    {
      Result = new PagedResult<MediaPersonListItemResponse>
      {
        Items = items,
        TotalCount = totalCount,
        Page = page,
        PageSize = pageSize,
      },
    };

  // "actor_id" -> "actor". The column names come from MediaCreditRoles' fixed list.
  private static string RoleName(MediaCreditRoles.CreditSource source) =>
    source.PersonColumn[..^"_id".Length];

  // Names sort by last name: the last word, after dropping Jr./Sr./II/III/IV. A one-word name
  // sorts as itself. person_key breaks ties so OFFSET paging never repeats or skips a row.
  private static (string Key, string Clause) BuildOrder(string? sort) =>
    sort switch
    {
      "name_desc" => ("name_desc", "ORDER BY last_name DESC, name DESC, person_key DESC"),
      "titles_desc" => (
        "titles_desc",
        "ORDER BY title_count DESC, last_name ASC, name ASC, person_key ASC"
      ),
      "titles_asc" => (
        "titles_asc",
        "ORDER BY title_count ASC, last_name ASC, name ASC, person_key ASC"
      ),
      _ => ("name_asc", "ORDER BY last_name ASC, name ASC, person_key ASC"),
    };

  // Escape LIKE wildcards so a typed "%" or "_" matches literally.
  private static string EscapeLike(string value) =>
    value
      .Replace("\\", "\\\\", StringComparison.Ordinal)
      .Replace("%", "\\%", StringComparison.Ordinal)
      .Replace("_", "\\_", StringComparison.Ordinal);

  // Field order must match the SELECT column order exactly.
  private sealed record PersonRow(
    string PersonKey,
    string Name,
    int TitleCount,
    string[] Roles,
    int TotalCount
  );

  private sealed record CachedPeoplePage(
    IReadOnlyList<MediaPersonListItemResponse> Items,
    int TotalCount
  );
}
