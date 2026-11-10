namespace ScreenDrafts.Modules.Movies.Features.Movies.SearchMediaPeople;

internal sealed class SearchMediaPeopleQueryHandler(IDbConnectionFactory dbConnectionFactory)
  : IQueryHandler<SearchMediaPeopleQuery, SearchMediaPeopleResponse>
{
  private const int MinQueryLength = 2;
  private const int DefaultLimit = 10;
  private const int MaxLimit = 25;

  public async Task<Result<SearchMediaPeopleResponse>> Handle(
    SearchMediaPeopleQuery request,
    CancellationToken cancellationToken
  )
  {
    if (!MediaCreditRoles.TryResolve(request.Role, out var sources))
    {
      return Result.Failure<SearchMediaPeopleResponse>(MediaErrors.InvalidCreditRole);
    }

    var term = request.Query?.Trim() ?? string.Empty;

    if (term.Length < MinQueryLength)
    {
      return Result.Success(new SearchMediaPeopleResponse { Items = [] });
    }

    var limit = request.Limit is < 1 or > MaxLimit ? DefaultLimit : request.Limit;

    await using var connection = await dbConnectionFactory.OpenConnectionAsync(cancellationToken);

    // Match people by name first, then count credits only for those matches. Table and column
    // names in the UNION come from MediaCreditRoles' fixed list; the search term and limit are
    // bound parameters. A person's key is their TMDb ID when they have one, otherwise their
    // IMDb ID (about four in five people predate the TMDb import and have tmdb_id = 0).
    // People with neither are skipped, as on the media detail page. Grouping by key folds
    // duplicate rows for one person into one result.
    var credits = string.Join(
      " UNION ALL ",
      sources.Select(s =>
        $"SELECT c.media_id, c.{s.PersonColumn} AS person_id FROM {s.Table} c JOIN matched mp ON mp.id = c.{s.PersonColumn}"
      )
    );

    var sql = $"""
      WITH matched AS (
        SELECT
            id,
            name,
            CASE WHEN tmdb_id > 0 THEN 't:' || tmdb_id::text ELSE 'i:' || imdb_id END AS person_key
        FROM movies.people
        WHERE name ILIKE @Search
          AND (tmdb_id > 0 OR COALESCE(imdb_id, '') <> '')
      ),
      credits AS (
        {credits}
      )
      SELECT
          mt.person_key                        AS {nameof(MediaPersonResponse.PersonKey)},
          MIN(mt.name)                         AS {nameof(MediaPersonResponse.Name)},
          COUNT(DISTINCT cr.media_id)::int     AS {nameof(MediaPersonResponse.TitleCount)}
      FROM matched mt
      JOIN credits cr ON cr.person_id = mt.id
      GROUP BY mt.person_key
      ORDER BY COUNT(DISTINCT cr.media_id) DESC, MIN(mt.name) ASC
      LIMIT @Limit
      """;

    // S2077: sql interpolates only the fixed credit-table fragments above; all values are bound parameters.
#pragma warning disable S2077
    var rows = await connection.QueryAsync<MediaPersonRow>(
      sql,
      new { Search = $"%{EscapeLike(term)}%", Limit = limit }
    );
#pragma warning restore S2077

    var items = rows.Select(r => new MediaPersonResponse
      {
        PersonKey = r.PersonKey,
        Name = r.Name,
        TitleCount = r.TitleCount,
      })
      .ToList();

    return Result.Success(new SearchMediaPeopleResponse { Items = items });
  }

  // Escape LIKE wildcards so a typed "%" or "_" matches literally.
  private static string EscapeLike(string value) =>
    value.Replace("\\", "\\\\", StringComparison.Ordinal)
      .Replace("%", "\\%", StringComparison.Ordinal)
      .Replace("_", "\\_", StringComparison.Ordinal);

  // Field order must match the SELECT column order exactly.
  private sealed record MediaPersonRow(string PersonKey, string Name, int TitleCount);
}
