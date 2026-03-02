namespace ScreenDrafts.Modules.Drafts.Features.Drafts.ListDrafts;

internal sealed class ListDraftsQueryHandler(IDbConnectionFactory connectionFactory)
    : IQueryHandler<ListDraftsQuery, PagedResult<ListDraftsResponse>>
{
  private const int MainFeedChannel = 0;
  private const int PatreonChannel = 1;

  private readonly IDbConnectionFactory _connectionFactory = connectionFactory;

  public async Task<Result<PagedResult<ListDraftsResponse>>> Handle(
    ListDraftsQuery request,
    CancellationToken cancellationToken)
  {
    ArgumentNullException.ThrowIfNull(request);

    await using var connection = await _connectionFactory.OpenConnectionAsync(cancellationToken);

    // 1. Base query - one row per draft part.
    const string baseSql =
      $"""
      SELECT
        dp.public_id AS {nameof(ListDraftsResponse.DraftPartPublicId)},
        d.public_id AS {nameof(ListDraftsResponse.DraftPublicId)},
        CASE
          WHEN (SELECT COUNT(*) FROM drafts.draft_parts dp2 WHERE dp2.draft_id = d.id) > 1
            THEN CONCAT(d.title, ' - Part ', dp.part_index)
          ELSE d.title
        END AS {nameof(ListDraftsResponse.Label)},
        d.draft_type AS {nameof(ListDraftsResponse.DraftType)},
        d.draft_status AS {nameof(ListDraftsResponse.DraftStatus)},
        dp.status AS {nameof(ListDraftsResponse.PartStatus)},
        dp.id as PartInternalId,
        d.id as DraftInternalId,
        EXISTS (
          SELECT 1
          FROM drafts.draft_part_participants dpp
          WHERE dpp.draft_part_id = dp.id
            AND dpp.participant_kind_value = 2
        ) AS {nameof(ListDraftsResponse.HasCommunityParticipant)},
        (
          SELECT COUNT(*)
          FROM drafts.picks pk
          WHERE pk.draft_part_id = dp.id
        ) AS {nameof(ListDraftsResponse.TotalPicks)}
      FROM drafts.draft_parts dp
      JOIN drafts.drafts d ON d.id = dp.draft_id
      WHERE 1 = 1
    """;

    var sqlBuilder = new StringBuilder(baseSql);
    var p = new DynamicParameters();

    // Date range filter
    if (request.FromDate.HasValue)
    {
      sqlBuilder.Append("""
              AND EXISTS (
                SELECT 1
                FROM drafts.draft_releases dr
                WHERE dr.part_id = dp.id
                  AND dr.release_date >= @FromDate
              )
      """);
      p.Add("fromDate", request.FromDate.Value.ToDateTime(TimeOnly.MinValue));
    }
    if (request.ToDate.HasValue)
    {
      sqlBuilder.Append("""
              AND EXISTS (
                SELECT 1
                FROM drafts.draft_releases dr
                WHERE dr.part_id = dp.id
                  AND dr.release_date <= @ToDate
              )
      """);
      p.Add("toDate", request.ToDate.Value.ToDateTime(TimeOnly.MinValue));
    }

    // Draft type filter
    if (request.DraftType.HasValue)
    {
      sqlBuilder.Append(" AND dp.draft_type = @DraftType");
      p.Add("draftType", request.DraftType.Value);
    }

    // Category filter
    // Categories belong to the draft not the draft part
    if (!string.IsNullOrWhiteSpace(request.CategoryPublicId))
    {
      sqlBuilder.Append(
        """
          AND EXISTS (
            SELECT 1
            FROM drafts.draft_categories dc
            JOIN drafts.categories c ON c.id = dc.category_id
            WHERE dc.draft_id = d.id
              AND c.public_id = @CategoryPublicId
          )
        """);
      p.Add("categoryPublicId", request.CategoryPublicId);
    }

    // Min/ max drafters filter
    if (request.MinDrafters.HasValue)
    {
      sqlBuilder.Append("""
              AND (
                SELECT COUNT(DISTINCT dpp.participant_id_value)
                FROM drafts.draft_part_participants dpp
                WHERE dpp.draft_part_id = dp.id
              ) BETWEEN @minDrafters AND @maxDrafters
      """);
      p.Add("minDrafters", request.MinDrafters ?? 0);
      p.Add("maxDrafters", request.MaxDrafters ?? int.MaxValue);
    }

    // Min/ max picks filter
    if (request.MinPicks.HasValue)
    {
      sqlBuilder.Append("""
              AND (
                SELECT COUNT(*)
                FROM drafts.picks pk
                WHERE pk.draft_part_id = dp.id
              ) BETWEEN @minPicks AND @maxPicks
      """);
      p.Add("minPicks", request.MinPicks ?? 0);
      p.Add("maxPicks", request.MaxPicks ?? int.MaxValue);
    }

    // Search query filter (title, drafter names, host names)
    if (!string.IsNullOrWhiteSpace(request.Q))
    {
      sqlBuilder.Append("""
              AND (
                d.title ILIKE '%' || @Q || '%'
                OR EXISTS (
                  SELECT 1
                  FROM drafts.draft_part_participants dpp
                  JOIN drafts.drafters dr ON dr.id = dpp.participant_id_value 
                                          AND dpp.participant_kind_value = 0
                  JOIN drafts.people pe ON pe.id = dr.person_id
                  WHERE dpp.draft_part_id = dp.id
                    AND pe.display_name ILIKE '%' || @Q || '%'
                )
                OR EXISTS (
                  SELECT 1
                  FROM drafts.draft_hosts dh
                  JOIN drafts.hosts h ON h.id = dh.host_id
                  JOIN drafts.people pe ON pe.id = h.person_id
                  WHERE dh.draft_part_id = dp.id
                    AND pe.display_name ILIKE '%' || @Q || '%'
                )
              )
      """);
      p.Add("Q", $"%{request.Q}%");
    }

    // Sorting - key on each part's earliest release date by default
    var dir = request.Dir?.ToLowerInvariant() == "desc" ? "DESC" : "ASC";
    var orderByClause = request.SortBy?.ToLowerInvariant() switch
    {
      "title" => $"d.title {dir}, dp.part_index {dir}",
      "episodenumber" =>
        $"""
        (SELECT dcr.episode_number
          FROM drafts.draft_channel_releases dcr
          WHERE dcr.draft_id = d.id
            AND dcr.episode_number = 0) {dir}
        """,
      "totalpicks" => $"(SELECT COUNT(*) FROM drafts.picks pk WHERE pk.draft_part_id = dp.id) {dir}",
      "date" => $"(SELECT MIN(dr.release_date) FROM drafts.draft_releases dr WHERE dr.part_id = dp.id) {dir}",
      _ => $"(SELECT MIN(dr.release_date) FROM drafts.draft_releases dr WHERE dr.part_id = dp.id) {dir}",
    };

    sqlBuilder.Append(CultureInfo.InvariantCulture, $" ORDER BY {orderByClause}");

    //Count before pagination
    var totalCount = await connection.ExecuteScalarAsync<int>(
      new CommandDefinition(
        $"SELECT COUNT(*) FROM ({sqlBuilder}) sub",
        p,
        cancellationToken: cancellationToken));

    var pageSize = Math.Min(request.PageSize, 100);
    var skip = (Math.Max(request.Page, 1) - 1) * pageSize;
    p.Add("pageSize", pageSize);
    p.Add("skip", skip);
    sqlBuilder.Append(" LIMIT @pageSize OFFSET @skip");

    // 2. Fetch paged part rows
    var rawRows = (await connection.QueryAsync<(
      string DraftPartPublicId,
      string DraftPublicId,
      string Label,
      int DraftType,
      int DraftStatus,
      int PartStatus,
      Guid PartInternalId,
      Guid DraftInternalId,
      bool HasCommunityParticipant,
      int TotalPicks)>(
      new CommandDefinition(sqlBuilder.ToString(), p, cancellationToken: cancellationToken)))
      .ToList();

    if (rawRows.Count == 0)
    {
      return Result.Success(new PagedResult<ListDraftsResponse>
      {
        Items = [],
        TotalCount = 0,
        Page = request.Page,
        PageSize = pageSize
      });
    }

    var partInternalIds = rawRows.Select(r => r.PartInternalId).ToArray();
    var allowedChannels = request.IncludePatreonOnly
      ? new[] { MainFeedChannel, PatreonChannel }
      : [MainFeedChannel];

    // 3. Releases
    const string releasesSql =
      """
      SELECT
        dr.part_id            AS PartId,
        dr.release_channel    AS ReleaseChannel,
        dcr.episode_number    AS EpisodeNumber,
        dr.release_date       AS ReleaseDate
      FROM drafts.draft_releases dr
      LEFT JOIN drafts.draft_channel_releases dcr
        ON dcr.draft_id = (
          SELECT dp.draft_id 
          FROM drafts.draft_parts dp
          WHERE dp.id = dr.part_id
        )
        AND dcr.release_channel = dr.release_channel
      WHERE dr.part_id = ANY(@partIds)
        AND dr.release_channel = ANY(@channels)
      ORDER BY dr.release_date ASC
      """;

    var releaseRows = await connection.QueryAsync<(
      Guid PartId,
      int ReleaseChannel,
      int? EpisodeNumber,
      DateOnly ReleaseDate)>(
      new CommandDefinition(
        releasesSql,
        new { partIds = partInternalIds, channels = allowedChannels },
        cancellationToken: cancellationToken));

    // 4. Participants
    const string participantsSql =
      """
      SELECT
        dpp.draft_part_id             AS PartId,
        dpp.participant_id_value      AS ParticipantIdValue,
        dpp.participant_kind_value    AS ParticipantKindValue,
        COALESCE(
          pe.display_name,
          pe.first_name || ' ' || pe.last_name,
          dt.name
        ) AS DisplayName
      FROM drafts.draft_part_participants dpp
      LEFT JOIN drafts.drafters dr            ON dr.id = dpp.participant_id_value 
                                             AND dpp.participant_kind_value = 0
      LEFT JOIN drafts.people pe              ON pe.id = dr.person_id
      LEFT JOIN drafts.drafter_teams dt       ON dt.id = dpp.participant_id_value 
                                              AND dpp.participant_kind_value = 1
      WHERE dpp.draft_part_id = ANY(@partIds)
        AND dpp.participant_kind_value IN (0, 1)
      """;

    var participantRows = await connection.QueryAsync<(
      Guid PartId,
      Guid ParticipantIdValue,
      int ParticipantKindValue,
      string DisplayName)>(
      new CommandDefinition(
        participantsSql,
        new { partIds = partInternalIds },
        cancellationToken: cancellationToken));

    // 5. Hosts
    const string hostsSql =
      """
      SELECT
        dh.draft_part_id AS PartId,
        h.public_id AS HostPublicId,
        COALESCE(
          pe.display_name,
          pe.first_name || ' ' || pe.last_name
        ) AS DisplayName,
        dh.role AS Role
      FROM drafts.draft_hosts dh
      JOIN drafts.hosts h ON h.id = dh.host_id
      JOIN drafts.people pe ON pe.id = h.person_id
      WHERE dh.draft_part_id = ANY(@partIds)
      ORDER BY dh.role ASC
      """;

    var hostRows = await connection.QueryAsync<(
      Guid PartId,
      string HostPublicId,
      string DisplayName,
      int Role)>(
      new CommandDefinition(
        hostsSql,
        new { partIds = partInternalIds },
        cancellationToken: cancellationToken));

    // 6. Assemble
    var releasePartId = releaseRows
      .GroupBy(r => r.PartId)
      .ToDictionary(
        g => g.Key,
        g => (IReadOnlyList<ListDraftsReleaseResponse>)[.. g.Select(r => new ListDraftsReleaseResponse
        {
          ReleaseChannel = r.ReleaseChannel,
          EpisodeNumber = r.EpisodeNumber,
          ReleaseDate = r.ReleaseDate
        })]);

    var participantPartId = participantRows
      .GroupBy(r => r.PartId)
      .ToDictionary(
        g => g.Key,
        g => (IReadOnlyList<ListDraftsParticipantResponse>)[.. g.Select(r => new ListDraftsParticipantResponse
        {
          ParticipantIdValue = r.ParticipantIdValue,
          ParticipantKindValue = r.ParticipantKindValue,
          DisplayName = r.DisplayName
        })]);

    var hostsByPartId = hostRows
      .GroupBy(r => r.PartId)
      .ToDictionary(
        g => g.Key,
        g => (IReadOnlyList<ListDraftsHostResponse>)[.. g.Select(r => new ListDraftsHostResponse
        {
          HostPublicId = r.HostPublicId,
          DisplayName = r.DisplayName,
          Role = r.Role
        })]);

    var items = rawRows.Select(r =>
    {
      var item = new ListDraftsResponse
      {
        DraftPartPublicId = r.DraftPartPublicId,
        DraftPublicId = r.DraftPublicId,
        Label = r.Label,
        DraftType = r.DraftType,
        DraftStatus = r.DraftStatus,
        PartStatus = r.PartStatus,
        HasCommunityParticipant = r.HasCommunityParticipant,
        TotalPicks = r.TotalPicks
      };

      item.SetReleases(releasePartId.GetValueOrDefault(r.PartInternalId, []));
      item.SetParticipants(participantPartId.GetValueOrDefault(r.PartInternalId, []));
      item.SetHosts(hostsByPartId.GetValueOrDefault(r.PartInternalId, []));
      return item;
    }).ToList();

    return Result.Success(new PagedResult<ListDraftsResponse>
    {
      Items = items,
      TotalCount = totalCount,
      Page = request.Page,
      PageSize = pageSize
    });
  }
}
