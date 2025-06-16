namespace ScreenDrafts.Modules.Drafts.Application.Drafts.Queries.ListDrafts;

internal sealed class ListDraftsQueryHandler(IDbConnectionFactory dbConnectionFactory)
  : IQueryHandler<ListDraftsQuery, IReadOnlyCollection<DraftResponse>>
{
  private readonly IDbConnectionFactory _dbConnectionFactory = dbConnectionFactory;

  [System.Diagnostics.CodeAnalysis.SuppressMessage("Globalization", "CA1308:Normalize strings to uppercase", Justification = "Reviewed")]
  public async Task<Result<IReadOnlyCollection<DraftResponse>>> Handle(
    ListDraftsQuery request,
    CancellationToken cancellationToken)
  {
    await using var connection = await _dbConnectionFactory.OpenConnectionAsync();

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
              d.id AS {nameof(DrafterResponse.Id)},
              d.name AS {nameof(DrafterResponse.Name)}
            FROM drafts.drafts_drafters dd
            JOIN drafts.drafters d ON dd.drafter_id = d.id
            WHERE dd.draft_id = ANY(@ids);
            """;

    const string hostsSql =
      $"""
            SELECT
              dh.hosted_drafts_id,
              h.id AS {nameof(HostResponse.Id)},
              h.host_name AS {nameof(HostResponse.Name)}
            FROM drafts.draft_host dh
            JOIN drafts.hosts h ON dh.hosts_id = h.id
            WHERE dh.hosted_drafts_id = ANY(@ids);
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

    var drafts = (await connection.QueryAsync<DraftResponse>(sql.ToString(), p)).ToList();
    var draftIds = drafts.Select(d => d.Id).ToArray();

    if (draftIds.Length == 0)
    {
      return drafts;
    }

    var drafters = await connection.QueryAsync<(Guid draft_id, Guid drafter_id, string drafter_name)>(draftersSql, new { ids = draftIds });
    var hosts = await connection.QueryAsync<(Guid draft_id, Guid host_id, string host_name)>(hostsSql, new { ids = draftIds });
    var releaseDates = await connection.QueryAsync<(Guid draft_id, DateTime release_date)>(releaseDatesSql, new { ids = draftIds });

    var draftMap = drafts.ToDictionary(d => d.Id);

    foreach (var (draftId, drafterId, name) in drafters)
    {
      if (draftMap.TryGetValue(draftId, out var draft))
      {
        draft.AddDrafter(new DrafterResponse(drafterId, name));
      }
    }

    foreach(var (draftId, hostId, name) in hosts)
    {
      if (draftMap.TryGetValue(draftId, out var draft))
      {
        draft.AddHost(new HostResponse(hostId, name));
      }
    }

    foreach(var (draftId, date) in releaseDates)
    {
      if (draftMap.TryGetValue(draftId, out var draft))
      {
        draft.AddReleaseDate(new ReleaseDateResponse(DateOnly.FromDateTime(date)));
      }
    }

    return draftMap.Values.ToList();
  }
}
