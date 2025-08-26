namespace ScreenDrafts.Modules.Drafts.Application.Drafts.Queries.ListDrafts;

internal sealed class ListDraftsQueryHandler(IDbConnectionFactory dbConnectionFactory)
  : IQueryHandler<ListDraftsQuery, PagedResult<DraftResponse>>
{
  private readonly IDbConnectionFactory _dbConnectionFactory = dbConnectionFactory;

  [System.Diagnostics.CodeAnalysis.SuppressMessage("Globalization", "CA1308:Normalize strings to uppercase", Justification = "Reviewed")]
  public async Task<Result<PagedResult<DraftResponse>>> Handle(
    ListDraftsQuery request,
    CancellationToken cancellationToken)
  {
    await using var connection = await _dbConnectionFactory.OpenConnectionAsync(cancellationToken);

    const string baseSql =
      $"""
                SELECT
                  d.id AS {nameof(DraftResponse.Id)},
                  d.title AS {nameof(DraftResponse.Title)},
                  d.episode_number AS {nameof(DraftResponse.EpisodeNumber)},
                  d.draft_type AS {nameof(DraftResponse.DraftType)},
                  d.total_picks AS {nameof(DraftResponse.TotalPicks)},
                  d.total_drafters AS {nameof(DraftResponse.TotalDrafters)},
                  d.total_hosts AS {nameof(DraftResponse.TotalHosts)},
                  d.draft_status AS {nameof(DraftResponse.DraftStatus)}
                FROM drafts.drafts d
                WHERE 1 = 1
                """;

    const string draftersSql =
      $"""
                SELECT
                  dd.draft_id,
                  d.id AS {nameof(DrafterDraftResponse.Id)},
                  d.person_id AS {nameof(DrafterDraftResponse.PersonId)},
                  p.display_name AS {nameof(DrafterDraftResponse.DisplayName)}
                FROM drafts.drafts_drafters dd
                JOIN drafts.drafters d ON dd.drafter_id = d.id
                INNER JOIN drafts.people p ON d.person_id = p.id
                WHERE dd.draft_id = ANY(@ids);
                """;

    const string primaryHostSql =
      $"""
                SELECT
                  dh.draft_id,
                  h.id AS {nameof(HostDraftResponse.Id)},
                  h.person_id AS {nameof(HostDraftResponse.PersonId)},
                  p.display_name AS {nameof(HostDraftResponse.DisplayName)}
                FROM drafts.draft_hosts dh
                JOIN drafts.hosts h ON dh.host_id = h.id
                JOIN drafts.people p ON h.person_id = p.id
                WHERE dh.draft_id = ANY(@ids) AND dh.role = 0;
                """;

    const string coHostsSql =
      $"""
                SELECT
                  dh.draft_id,
                  h.id AS {nameof(HostDraftResponse.Id)},
                  h.person_id AS {nameof(HostDraftResponse.PersonId)},
                  p.display_name AS {nameof(HostDraftResponse.DisplayName)}
                FROM drafts.draft_hosts dh
                JOIN drafts.hosts h ON dh.host_id = h.id
                JOIN drafts.people p ON h.person_id = p.id
                WHERE dh.draft_id = ANY(@ids) AND dh.role = 1;
                """;

    const string releaseDatesSql =
      $"""
                SELECT
                  rd.draft_id,
                  rd.release_date AS {nameof(ReleaseDateResponse.ReleaseDate)}
                FROM drafts.draft_release_date rd
                WHERE rd.draft_id = ANY(@ids);
                """;

    var sql = new StringBuilder(baseSql);
    var p = new DynamicParameters();

    // Apply filters based on the request parameters
    // Date Range
    if (request.FromDate.HasValue)
    {
      sql.Append(""" 
             AND EXISTS (
                SELECT 1
                FROM drafts.draft_release_date rd
                WHERE rd.draft_id = d.id
                  AND rd.release_date >= @fromDate
            )
            """);
      p.Add("fromDate", request.FromDate.Value.ToDateTime(TimeOnly.MinValue));
    }

    if (request.ToDate.HasValue)
    {
      sql.Append(""" 
             AND EXISTS (
                SELECT 1
                FROM drafts.draft_release_date rd
                WHERE rd.draft_id = d.id
                  AND rd.release_date <= @toDate
            )
            """);
      p.Add("toDate", request.ToDate.Value.ToDateTime(TimeOnly.MaxValue));
    }

    // Draft Type
    if (request.DraftType?.Any() == true)
    {
      sql.Append(" AND d.draft_type = ANY(@draftTypes)");
      p.Add("draftTypes", request.DraftType!.ToArray());
    }

    // Drafters Count
    if (request.MinDrafters.HasValue)
    {
      sql.Append(" AND d.total_drafters >= @minDrafters");
      p.Add("minDrafters", request.MinDrafters);
    }

    if (request.MaxDrafters.HasValue)
    {
      sql.Append(" AND d.total_drafters <= @maxDrafters");
      p.Add("maxDrafters", request.MaxDrafters);
    }

    // Picks Count
    if (request.MinPicks.HasValue)
    {
      sql.Append(" AND d.total_picks >= @minPicks");
      p.Add("minPicks", request.MinPicks);
    }

    if (request.MaxPicks.HasValue)
    {
      sql.Append(" AND d.total_picks <= @maxPicks");
      p.Add("maxPicks", request.MaxPicks);
    }

    // Search Query
    if (!string.IsNullOrWhiteSpace(request.Q))
    {
      sql.Append(
        """
         AND (
           d.title ILIKE '%' || @q || '%')
        """);
      p.Add("q", request.Q);
    }

    if (!request.IsPatreonOnly)
    {
      sql.Append(" AND d.is_patreon_only = FALSE");
    }

    // Sorting
    var sortColumn = request.Sort?.ToLowerInvariant() switch
    {
      "title" => "d.title",
      "episodenumber" => "d.episode_number",
      "totalpicks" => "d.total_picks",
      "date" => "(SELECT MIN(rd.release_date) FROM drafts.draft_release_date rd WHERE rd.draft_id = d.id)",
      _ => "(SELECT MIN(rd.release_date) FROM drafts.draft_release_date rd WHERE rd.draft_id = d.id)"
    };

    var dir = request.Dir?.ToLowerInvariant() == "desc" ? "DESC" : "ASC";

    sql.AppendFormat(CultureInfo.InvariantCulture, " ORDER BY {0} {1}", sortColumn, dir);

    var baseSqlWithFilters = sql.ToString();

    var totalCount = await connection.ExecuteScalarAsync<int>($"""
      SELECT COUNT(*)
      FROM ({baseSqlWithFilters})
      """, p);

    var skip = (request.Page <= 1 ? 0 : request.Page - 1) * request.PageSize;
    p.Add("pageSize", request.PageSize > 100 ? 100 : request.PageSize); // Limit to 100 for performance
    p.Add("skip", skip);

    sql.Append(" LIMIT @pageSize OFFSET @skip");

    var drafts = (await connection.QueryAsync<DraftResponse>(sql.ToString(), p)).ToList();
    var draftIds = drafts.Select(d => d.Id).ToArray();

    if (draftIds.Length == 0)
    {
      return new PagedResult<DraftResponse>(
        Items: drafts,
        Total: totalCount,
        Page: request.Page,
        PageSize: request.PageSize);
    }

    var drafters = await connection.QueryAsync<(Guid draft_id, Guid drafter_id, Guid person_id, string display_name)>(draftersSql, new { ids = draftIds });
    var primaryHost = await connection.QueryAsync<(Guid draft_id, Guid host_id, Guid person_id, string display_name)>(primaryHostSql, new { ids = draftIds });
    var coHosts = await connection.QueryAsync<(Guid draft_id, Guid host_id, Guid person_id, string display_name)>(coHostsSql, new { ids = draftIds });
    var releaseDates = await connection.QueryAsync<(Guid draft_id, DateTime release_date)>(releaseDatesSql, new { ids = draftIds });

    var draftMap = drafts.ToDictionary(d => d.Id);

    foreach (var (draftId, drafterId, personId, displayName) in drafters)
    {
      if (draftMap.TryGetValue(draftId, out var draft))
      {
        draft.AddDrafter(new DrafterDraftResponse(drafterId, personId, displayName));
      }
    }

    foreach (var (draftId, hostId, personId, displayName) in primaryHost)
    {
      if (draftMap.TryGetValue(draftId, out var draft))
      {
        draft.SetPrimaryHost(new HostDraftResponse(hostId, personId, displayName));
      }
    }

    foreach (var (draftId, hostId, personId, displayName) in coHosts)
    {
      if (draftMap.TryGetValue(draftId, out var draft))
      {
        draft.AddCoHost(new HostDraftResponse(hostId, personId, displayName));
      }
    }

    foreach (var (draftId, date) in releaseDates)
    {
      if (draftMap.TryGetValue(draftId, out var draft))
      {
        draft.AddReleaseDate(new ReleaseDateResponse(DateOnly.FromDateTime(date)));
        draft.PopulateReleaseDatesFromRaw();
      }
    }

    return new PagedResult<DraftResponse>(
      Items: [.. draftMap.Values],
      Total: totalCount,
      Page: request.Page,
      PageSize: request.PageSize);
  }
}
