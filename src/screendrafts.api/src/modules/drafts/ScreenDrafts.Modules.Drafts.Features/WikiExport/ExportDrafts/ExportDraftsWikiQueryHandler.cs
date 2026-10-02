namespace ScreenDrafts.Modules.Drafts.Features.WikiExport.ExportDrafts;

// ── Handler ───────────────────────────────────────────────────────────────

internal sealed class ExportDraftsWikiQueryHandler(IDbConnectionFactory connectionFactory)
  : IQueryHandler<ExportDraftsWikiQuery, ExportWikiResponse>
{
  private const int MainFeedChannel = 0;

  private readonly IDbConnectionFactory _connectionFactory = connectionFactory;

  public async Task<Result<ExportWikiResponse>> Handle(
    ExportDraftsWikiQuery request,
    CancellationToken cancellationToken
  )
  {
    ArgumentNullException.ThrowIfNull(request);

    await using var connection = await _connectionFactory.OpenConnectionAsync(cancellationToken);

    async Task<List<T>> ReadAsync<T>(string sql, object parameters) =>
      (
        await connection.QueryAsync<T>(
          new CommandDefinition(
            commandText: sql,
            parameters: parameters,
            cancellationToken: cancellationToken
          )
        )
      ).ToList();

    var requested = request.DraftPublicIds.Distinct(StringComparer.Ordinal).ToList();

    var drafts = await ReadAsync<DraftRow>(
      """
      SELECT
        d.id         AS DraftId,
        d.public_id  AS DraftPublicId,
        d.title      AS Title,
        c.id         AS CampaignId,
        c.name       AS CampaignName
      FROM drafts.drafts d
      LEFT JOIN drafts.campaigns c ON c.id = d.campaign_id AND c.is_deleted = FALSE
      WHERE d.public_id = ANY(@PublicIds)
        AND d.is_deleted = FALSE
        AND EXISTS (
          SELECT 1
          FROM drafts.draft_parts dp
          JOIN drafts.draft_releases dr
            ON dr.part_id = dp.id
           AND dr.release_channel = @Channel
          WHERE dp.draft_id = d.id
        )
      """,
      new { PublicIds = requested.ToArray(), Channel = MainFeedChannel }
    );

    if (drafts.Count == 0)
    {
      return Result.Failure<ExportWikiResponse>(WikiExportErrors.NoDraftsFound);
    }

    var draftIds = drafts.Select(d => d.DraftId).ToArray();

    var parts = await ReadAsync<PartRow>(
      """
      SELECT dp.id AS PartId, dp.draft_id AS DraftId, dp.part_index AS PartIndex
      FROM drafts.draft_parts dp
      JOIN drafts.draft_releases dr
        ON dr.part_id = dp.id
       AND dr.release_channel = @Channel
      WHERE dp.draft_id = ANY(@DraftIds)
      """,
      new { DraftIds = draftIds, Channel = MainFeedChannel }
    );

    var partIds = parts.Select(p => p.PartId).ToArray();

    var episodes = await ReadAsync<EpisodeRow>(
      """
      SELECT dcr.draft_id AS DraftId, dcr.episode_number AS EpisodeNumber
      FROM drafts.draft_channel_releases dcr
      WHERE dcr.draft_id = ANY(@DraftIds)
        AND dcr.release_channel = @Channel
      """,
      new { DraftIds = draftIds, Channel = MainFeedChannel }
    );

    var releases = await ReadAsync<ReleaseRow>(
      """
      SELECT dr.part_id AS PartId, dr.release_date AS ReleaseDate
      FROM drafts.draft_releases dr
      WHERE dr.part_id = ANY(@PartIds)
        AND dr.release_channel = @Channel
      """,
      new { PartIds = partIds, Channel = MainFeedChannel }
    );

    var participants = await ReadAsync<ParticipantRow>(
      """
      SELECT
        dpp.id                       AS ParticipantRowId,
        dpp.draft_part_id            AS PartId,
        dpp.participant_id_value     AS ParticipantValue,
        dpp.participant_kind_value   AS ParticipantKind,
        COALESCE(pe.display_name, dt.name, @CommunityName) AS DisplayName
      FROM drafts.draft_part_participants dpp
      LEFT JOIN drafts.drafters dr       ON dr.id = dpp.participant_id_value
                                        AND dpp.participant_kind_value = 0
      LEFT JOIN drafts.people pe         ON pe.id = dr.person_id
      LEFT JOIN drafts.drafter_teams dt  ON dt.id = dpp.participant_id_value
                                        AND dpp.participant_kind_value = 1
      WHERE dpp.draft_part_id = ANY(@PartIds)
      """,
      new { PartIds = partIds, CommunityName = WikiText.PatreonMembers }
    );

    var hosts = await ReadAsync<HostRow>(
      """
      SELECT
        dh.draft_part_id AS PartId,
        dh.role          AS Role,
        COALESCE(pe.display_name, pe.first_name || ' ' || pe.last_name) AS DisplayName
      FROM drafts.draft_hosts dh
      JOIN drafts.hosts h   ON h.id = dh.host_id
      JOIN drafts.people pe ON pe.id = h.person_id
      WHERE dh.draft_part_id = ANY(@PartIds)
      ORDER BY dh.role, DisplayName
      """,
      new { PartIds = partIds }
    );

    var picks = await ReadAsync<PickRow>(
      """
      SELECT
        pk.id                          AS PickId,
        pk.draft_part_id               AS PartId,
        pk.position                    AS Position,
        pk.play_order                  AS PlayOrder,
        COALESCE(sd.index, 0)          AS SubDraftIndex,
        pk.played_by_participant_id    AS ParticipantRowId,
        pk.movie_id                    AS MovieId,
        m.movie_title                  AS Title,
        m.year                         AS Year,
        (co.id IS NOT NULL)            AS WasCommissionerOverride
      FROM drafts.picks pk
      JOIN drafts.movies m                       ON m.id = pk.movie_id
      LEFT JOIN drafts.commissioner_overrides co ON co.pick_id = pk.id
      LEFT JOIN drafts.sub_drafts sd             ON sd.id = pk.sub_draft_id
      WHERE pk.draft_part_id = ANY(@PartIds)
      """,
      new { PartIds = partIds }
    );

    var vetoes = await ReadAsync<VetoRow>(
      """
      SELECT
        v.target_pick_id              AS PickId,
        v.sequence                    AS Sequence,
        v.issued_by_participant_id    AS IssuedByParticipantRowId,
        v.is_overridden               AS IsOverridden,
        vo.issued_by_participant_id   AS OverriddenByParticipantRowId
      FROM drafts.vetoes v
      JOIN drafts.picks pk              ON pk.id = v.target_pick_id
      LEFT JOIN drafts.veto_overrides vo ON vo.veto_id = v.id
      WHERE pk.draft_part_id = ANY(@PartIds)
      """,
      new { PartIds = partIds }
    );

    var trivia = await ReadAsync<TriviaRow>(
      """
      SELECT
        tr.draft_part_id    AS PartId,
        tr.position         AS Position,
        tr.questions_won    AS QuestionsWon,
        tr.participant_id   AS ParticipantValue,
        tr.participant_kind AS ParticipantKind
      FROM drafts.trivia_results tr
      WHERE tr.draft_part_id = ANY(@PartIds)
      ORDER BY tr.position
      """,
      new { PartIds = partIds }
    );

    var categories = await ReadAsync<CategoryRow>(
      """
      SELECT dc.draft_id AS DraftId, c.name AS Name
      FROM drafts.draft_categories dc
      JOIN drafts.categories c ON c.id = dc.category_id
      WHERE dc.draft_id = ANY(@DraftIds)
        AND c.is_deleted = FALSE
      ORDER BY c.name
      """,
      new { DraftIds = draftIds }
    );

    // Every main-feed release, so "previous / next episode" can be worked out by air order.
    var timeline = await ReadAsync<TimelineRow>(
      """
      SELECT
        d.id                AS DraftId,
        d.title             AS Title,
        dp.part_index       AS PartIndex,
        (
          SELECT COUNT(*)::int FROM drafts.draft_parts x WHERE x.draft_id = d.id
        )                   AS PartCount,
        dr.release_date     AS ReleaseDate,
        dcr.episode_number  AS EpisodeNumber
      FROM drafts.draft_releases dr
      JOIN drafts.draft_parts dp ON dp.id = dr.part_id
      JOIN drafts.drafts d       ON d.id = dp.draft_id
      LEFT JOIN drafts.draft_channel_releases dcr
        ON dcr.draft_id = d.id
       AND dcr.release_channel = @Channel
      WHERE dr.release_channel = @Channel
        AND d.is_deleted = FALSE
      """,
      new { Channel = MainFeedChannel }
    );

    var campaignIds = drafts
      .Where(d => d.CampaignId.HasValue)
      .Select(d => d.CampaignId!.Value)
      .Distinct()
      .ToArray();

    // Every draft in the same campaigns, so "last / next in series" can be worked out by air order.
    var campaignDrafts = await ReadAsync<CampaignDraftRow>(
      """
      SELECT
        d.id                  AS DraftId,
        d.campaign_id         AS CampaignId,
        d.title               AS Title,
        MIN(dr.release_date)  AS FirstRelease
      FROM drafts.drafts d
      JOIN drafts.draft_parts dp ON dp.draft_id = d.id
      JOIN drafts.draft_releases dr
        ON dr.part_id = dp.id
       AND dr.release_channel = @Channel
      WHERE d.campaign_id = ANY(@CampaignIds)
        AND d.is_deleted = FALSE
      GROUP BY d.id, d.campaign_id, d.title
      """,
      new { CampaignIds = campaignIds, Channel = MainFeedChannel }
    );

    var predictionSets = await ReadAsync<PredictionSetRow>(
      """
      SELECT
        ps.id                 AS SetId,
        ps.draft_part_id      AS PartId,
        c.display_name        AS ContestantName,
        ps.submitted_at_utc   AS SubmittedAtUtc
      FROM drafts.draft_prediction_sets ps
      JOIN drafts.prediction_contestants c ON c.id = ps.contestant_id
      WHERE ps.draft_part_id = ANY(@PartIds)
      """,
      new { PartIds = partIds }
    );

    var predictionModes = await ReadAsync<PredictionModeRow>(
      """
      SELECT r.draft_part_id AS PartId, r.prediction_mode AS PredictionMode
      FROM drafts.draft_part_prediction_rules r
      WHERE r.draft_part_id = ANY(@PartIds)
      """,
      new { PartIds = partIds }
    );

    var setIds = predictionSets.Select(s => s.SetId).ToArray();

    // Hits first, then the rest in the contestant's own order.
    var predictionEntries = await ReadAsync<PredictionEntryRow>(
      """
      SELECT
        e.set_id        AS SetId,
        e.media_title   AS Title,
        e.is_correct    AS IsCorrect,
        e.order_index   AS OrderIndex
      FROM drafts.prediction_entries e
      WHERE e.set_id = ANY(@SetIds)
      ORDER BY e.set_id, (e.is_correct IS TRUE) DESC, e.order_index, e.media_title
      """,
      new { SetIds = setIds }
    );

    // Season points as of this part's air date: scored results up to it, plus carryovers.
    var standings = await ReadAsync<StandingRow>(
      """
      SELECT
        ps.id AS SetId,
        (
          COALESCE((
            SELECT SUM(r2.points_awarded)
            FROM drafts.draft_prediction_sets s2
            JOIN drafts.prediction_results r2 ON r2.set_id = s2.id
            JOIN drafts.draft_releases dr2
              ON dr2.part_id = s2.draft_part_id
             AND dr2.release_channel = @Channel
            WHERE s2.contestant_id = ps.contestant_id
              AND s2.season_id = ps.season_id
              AND dr2.release_date <= (
                SELECT MIN(dr.release_date)
                FROM drafts.draft_releases dr
                WHERE dr.part_id = ps.draft_part_id
                  AND dr.release_channel = @Channel
              )
          ), 0)
          + COALESCE((
            SELECT SUM(pc.points)
            FROM drafts.prediction_carryovers pc
            WHERE pc.contestant_id = ps.contestant_id
              AND pc.season_id = ps.season_id
          ), 0)
        )::int AS Points
      FROM drafts.draft_prediction_sets ps
      WHERE ps.draft_part_id = ANY(@PartIds)
      """,
      new { PartIds = partIds, Channel = MainFeedChannel }
    );

    var data = new Data
    {
      PartsByDraft = parts
        .GroupBy(p => p.DraftId)
        .ToDictionary(g => g.Key, g => g.OrderBy(p => p.PartIndex).ToList()),
      EpisodeByDraft = episodes.ToDictionary(e => e.DraftId, e => e.EpisodeNumber),
      ReleaseByPart = releases.ToDictionary(r => r.PartId, r => r.ReleaseDate),
      ParticipantsByPart = participants
        .GroupBy(p => p.PartId)
        .ToDictionary(g => g.Key, g => g.ToList()),
      NameByRow = participants.ToDictionary(p => p.ParticipantRowId, p => p.DisplayName),
      HostsByPart = hosts.GroupBy(h => h.PartId).ToDictionary(g => g.Key, g => g.ToList()),
      PicksByPart = picks
        .GroupBy(p => p.PartId)
        .ToDictionary(
          g => g.Key,
          g => g.OrderBy(p => p.SubDraftIndex).ThenBy(p => p.PlayOrder).ToList()
        ),
      VetoesByPick = vetoes
        .GroupBy(v => v.PickId)
        .ToDictionary(g => g.Key, g => g.OrderBy(v => v.Sequence).ToList()),
      TriviaByPart = trivia.GroupBy(t => t.PartId).ToDictionary(g => g.Key, g => g.ToList()),
      CategoriesByDraft = categories
        .GroupBy(c => c.DraftId)
        .ToDictionary(g => g.Key, g => g.Select(c => c.Name).ToList()),
      CampaignDrafts = campaignDrafts
        .GroupBy(c => c.CampaignId)
        .ToDictionary(
          g => g.Key,
          g => g.OrderBy(c => c.FirstRelease).ThenBy(c => c.Title).ToList()
        ),
      SetsByPart = predictionSets
        .GroupBy(s => s.PartId)
        .ToDictionary(g => g.Key, g => g.OrderBy(s => s.SubmittedAtUtc).ToList()),
      ModeByPart = predictionModes.ToDictionary(m => m.PartId, m => m.PredictionMode),
      EntriesBySet = predictionEntries
        .GroupBy(e => e.SetId)
        .ToDictionary(g => g.Key, g => g.ToList()),
      PointsBySet = standings.ToDictionary(s => s.SetId, s => s.Points),
      Timeline =
      [
        .. timeline
          .OrderBy(t => t.ReleaseDate)
          .ThenBy(t => t.EpisodeNumber ?? int.MaxValue)
          .ThenBy(t => t.PartIndex),
      ],
    };

    // Keep the order the admin selected them in.
    var pages = drafts
      .OrderBy(d => requested.IndexOf(d.DraftPublicId))
      .Select(d => new WikiPage(d.Title, RenderDraft(d, data)))
      .ToList();

    return Result.Success(WikiText.ToResponse(pages, "drafts"));
  }

  // ── Rendering ───────────────────────────────────────────────────────────

  private static string RenderDraft(DraftRow draft, Data data)
  {
    var parts = data.PartsByDraft.GetValueOrDefault(draft.DraftId, []);
    var partCount = parts.Count;
    var episode = data.EpisodeByDraft.GetValueOrDefault(draft.DraftId);

    string NameOf(Guid? rowId) =>
      rowId is { } id && data.NameByRow.TryGetValue(id, out var name) ? name : "Unknown";

    var draftPicks = parts
      .SelectMany(p => data.PicksByPart.GetValueOrDefault(p.PartId, []))
      .ToList();

    var landedCount = draftPicks.Count(p =>
      !p.WasCommissionerOverride
      && !WikiText.IsFinallyVetoed(data.VetoesByPick.GetValueOrDefault(p.PickId, []))
    );

    // Drafters (and teams), in the order they first appear in the draft.
    var drafterNames = parts
      .SelectMany(part =>
      {
        var partPicks = data.PicksByPart.GetValueOrDefault(part.PartId, []);

        return data
          .ParticipantsByPart.GetValueOrDefault(part.PartId, [])
          .Where(p => p.ParticipantKind != WikiText.CommunityKind)
          .OrderBy(p =>
            partPicks
              .Where(pk => pk.ParticipantRowId == p.ParticipantRowId)
              .Select(pk => pk.PlayOrder)
              .DefaultIfEmpty(int.MaxValue)
              .Min()
          )
          .ThenBy(p => p.DisplayName, StringComparer.OrdinalIgnoreCase)
          .Select(p => p.DisplayName);
      })
      .Distinct(StringComparer.OrdinalIgnoreCase)
      .ToList();

    var airDates = parts
      .Where(p => data.ReleaseByPart.ContainsKey(p.PartId))
      .Select(p => WikiText.FormatDate(data.ReleaseByPart[p.PartId]))
      .ToList();

    var primaryRole = ScreenDrafts.Modules.Drafts.Domain.Hosts.HostRole.Primary.Value;

    string HostLines(Func<HostRow, bool> include) =>
      string.Join(
        "<br>",
        parts
          .SelectMany(part =>
            data.HostsByPart.GetValueOrDefault(part.PartId, [])
              .Where(include)
              .Select(h => (h.DisplayName, part.PartIndex))
          )
          .GroupBy(e => e.DisplayName)
          .Select(g =>
            WikiText.Link(g.Key) + WikiText.PartSuffix([.. g.Select(e => e.PartIndex)], partCount)
          )
      );

    string Neighbors(bool previous)
    {
      var labels = new List<string>();
      var step = previous ? -1 : 1;

      foreach (var part in parts)
      {
        var index = data.Timeline.FindIndex(t =>
          t.DraftId == draft.DraftId && t.PartIndex == part.PartIndex
        );

        if (index < 0)
        {
          continue;
        }

        for (var i = index + step; i >= 0 && i < data.Timeline.Count; i += step)
        {
          var other = data.Timeline[i];

          if (other.DraftId == draft.DraftId)
          {
            continue;
          }

          labels.Add(
            other.PartCount > 1
              ? $"[[{other.Title}]] Part {WikiText.Roman(other.PartIndex)}"
              : $"[[{other.Title}]]"
          );
          break;
        }
      }

      return string.Join("<br><br>", labels.Distinct());
    }

    string SeriesNeighbor(bool previous)
    {
      if (
        draft.CampaignId is not { } campaignId
        || !data.CampaignDrafts.TryGetValue(campaignId, out var siblings)
      )
      {
        return string.Empty;
      }

      var index = siblings.FindIndex(s => s.DraftId == draft.DraftId);
      var other = previous ? index - 1 : index + 1;

      return index >= 0 && other >= 0 && other < siblings.Count
        ? WikiText.Link(siblings[other].Title)
        : string.Empty;
    }

    var sb = new StringBuilder();

    void Line(string text) => sb.Append(text).Append('\n');

    Line("{{Episodes");
    Line(WikiText.Param("title", string.Empty));
    Line(
      WikiText.Param(
        "episodeNumber",
        episode?.ToString(System.Globalization.CultureInfo.InvariantCulture) ?? string.Empty
      )
    );
    Line(WikiText.Param("image", string.Empty));
    Line(WikiText.Param("airDate", string.Join("<br>", airDates)));
    Line(WikiText.Param("drafters", string.Join("<br>", drafterNames.Select(WikiText.Link))));
    Line(WikiText.Param("trivia", RenderTrivia(parts, data)));
    Line(WikiText.Param("commish", HostLines(h => h.Role == primaryRole)));
    Line(WikiText.Param("cocommish", HostLines(h => h.Role != primaryRole)));
    Line(WikiText.Param("length", string.Empty));
    Line(WikiText.Param("previousEpisode", Neighbors(previous: true)));
    Line(WikiText.Param("nextEpisode", Neighbors(previous: false)));

    if (draft.CampaignName is not null)
    {
      Line(WikiText.Param("NameOfSeries", $"'''{draft.CampaignName}'''"));
      Line(WikiText.Param("LastInSeries", SeriesNeighbor(previous: true)));
      Line(WikiText.Param("NextInSeries", SeriesNeighbor(previous: false)));
    }

    sb.Append("}}");

    var episodeText = episode is { } number
      ? $"the {WikiText.Ordinal(number)} episode"
      : "an episode";
    sb.Append(
      CultureInfo.InvariantCulture,
      $"'''{draft.Title}''' is {episodeText} of [[Screen Drafts]]."
    );

    if (drafterNames.Count > 0)
    {
      var linked = drafterNames.Select(WikiText.Link).ToList();
      sb.Append(
        CultureInfo.InvariantCulture,
        $" {WikiText.JoinNames(linked)} drafted {landedCount} movies."
      );
    }

    sb.Append("\n\n");
    Line("The following films were drafted:");
    Line(string.Empty);

    foreach (var part in parts)
    {
      if (partCount > 1)
      {
        Line($"==Part {WikiText.Roman(part.PartIndex)}==");
        Line(string.Empty);
      }

      foreach (var pick in data.PicksByPart.GetValueOrDefault(part.PartId, []))
      {
        Line(RenderPick(pick, data.VetoesByPick.GetValueOrDefault(pick.PickId, []), NameOf));
        Line(string.Empty);
      }
    }

    Line("==Predictions==");
    Line(string.Empty);
    Line(RenderPredictions(parts, data));
    Line(string.Empty);
    Line("==References==");
    Line("<references />");

    var releaseDates = parts
      .Where(p => data.ReleaseByPart.ContainsKey(p.PartId))
      .Select(p => data.ReleaseByPart[p.PartId])
      .ToList();

    if (releaseDates.Count > 0)
    {
      Line($"[[Category:{releaseDates.Min().Year} Drafts]]");
    }

    foreach (var category in data.CategoriesByDraft.GetValueOrDefault(draft.DraftId, []))
    {
      Line($"[[Category:{category}]]");
    }

    return sb.ToString();
  }

  private static string RenderPick(
    PickRow pick,
    IReadOnlyList<VetoRow> vetoes,
    Func<Guid?, string> nameOf
  )
  {
    var core =
      $"{pick.Position}. {WikiText.Link(pick.Title)} by {WikiText.Link(nameOf(pick.ParticipantRowId))}";
    var chain = WikiText.OverrideChain(vetoes, nameOf);

    if (pick.WasCommissionerOverride)
    {
      return $"<s>{core}</s> {WikiText.CommissionerOverrideNote}";
    }

    if (WikiText.IsFinallyVetoed(vetoes))
    {
      var prefix = chain.Length > 0 ? chain + " " : string.Empty;
      return $"<s>{core}</s> {prefix}vetoed by {WikiText.Link(nameOf(vetoes[^1].IssuedByParticipantRowId))}";
    }

    return chain.Length > 0 ? $"{core} {chain}" : core;
  }

  // One table per part that has prediction sets. Green = hit, gray = miss.
  // Nothing scored yet means the predictions were never revealed.
  private static string RenderPredictions(IReadOnlyList<PartRow> parts, Data data)
  {
    var sections = new List<string>();

    foreach (var part in parts)
    {
      var sets = data.SetsByPart.GetValueOrDefault(part.PartId, []);

      if (sets.Count == 0)
      {
        continue;
      }

      var columns = sets.Select(s => data.EntriesBySet.GetValueOrDefault(s.SetId, [])).ToList();

      // Ordered modes (OrderedTopN / OrderedAll) carry a position per entry: number every pick,
      // highest slot first, and show all of them whether or not they were right.
      // Without a rules row, fall back to whether the entries carry positions.
      var ordered = data.ModeByPart.TryGetValue(part.PartId, out var mode)
        ? mode == PredictionMode.OrderedTopN.Value || mode == PredictionMode.OrderedAll.Value
        : columns.Any(c => c.Any(e => e.OrderIndex.HasValue));

      if (ordered)
      {
        columns = columns
          .Select(c => c.OrderByDescending(e => e.OrderIndex ?? int.MinValue).ToList())
          .ToList();
      }

      string body;

      if (columns.All(c => c.All(e => e.IsCorrect is null)))
      {
        body = "Predictions were made but not revealed.";
      }
      else
      {
        var sb = new StringBuilder();
        sb.Append("{| class=\"fandom-table\"\n|+Predictions\n");

        foreach (var set in sets)
        {
          sb.Append('!').Append(WikiText.FirstToken(set.ContestantName)).Append('\n');
        }

        var rowCount = columns.Max(c => c.Count);

        for (var row = 0; row < rowCount; row++)
        {
          sb.Append("|-\n");

          foreach (var column in columns)
          {
            if (row < column.Count)
            {
              var entry = column[row];
              var color = entry.IsCorrect == true ? "lightgreen" : "gray";
              var number =
                ordered && entry.OrderIndex is { } position ? $"{position}. " : string.Empty;
              sb.Append(
                CultureInfo.InvariantCulture,
                $"| bgcolor=\"{color}\" |{number}{WikiText.Link(entry.Title)}\n"
              );
            }
            else
            {
              sb.Append("| bgcolor=\"gray\" |\n");
            }
          }
        }

        sb.Append(
          CultureInfo.InvariantCulture,
          $"|-\n| colspan=\"{sets.Count}\" |'''CURRENT STANDINGS'''\n|-\n"
        );

        foreach (var set in sets)
        {
          sb.Append(
            CultureInfo.InvariantCulture,
            $"|'''{WikiText.FirstToken(set.ContestantName)}'''\n"
          );
        }

        sb.Append("|-\n");

        foreach (var set in sets)
        {
          sb.Append('|')
            .Append(
              data.PointsBySet.GetValueOrDefault(set.SetId)
                .ToString(System.Globalization.CultureInfo.InvariantCulture)
            )
            .Append('\n');
        }

        sb.Append("|}");
        body = sb.ToString();
      }

      sections.Add(
        parts.Count > 1 ? $"===Part {WikiText.Roman(part.PartIndex)}===\n\n{body}" : body
      );
    }

    return sections.Count == 0
      ? "<!-- TODO: no predictions recorded for this draft -->"
      : string.Join("\n\n", sections);
  }

  // Two drafters in a part: "Matt 3-0". Otherwise the top two placings: "1st: William<br>2nd: Drea".
  // Multi-part drafts get a bold part heading per block.
  private static string RenderTrivia(IReadOnlyList<PartRow> parts, Data data)
  {
    var blocks = new List<string>();

    foreach (var part in parts)
    {
      var results = data.TriviaByPart.GetValueOrDefault(part.PartId, []);

      if (results.Count == 0)
      {
        continue;
      }

      var participants = data.ParticipantsByPart.GetValueOrDefault(part.PartId, []);

      string NameFor(TriviaRow t) =>
        participants
          .Where(p =>
            p.ParticipantValue == t.ParticipantValue && p.ParticipantKind == t.ParticipantKind
          )
          .Select(p => WikiText.FirstToken(p.DisplayName))
          .FirstOrDefault()
        ?? "Unknown";

      var competitors = participants.Count(p => p.ParticipantKind != WikiText.CommunityKind);

      string body =
        competitors == 2 && results.Count >= 2
          ? $"{NameFor(results[0])} {results[0].QuestionsWon}-{results[1].QuestionsWon}"
          : string.Join(
            "<br>",
            results.Take(2).Select(t => $"{WikiText.Ordinal(t.Position)}: {NameFor(t)}")
          );

      blocks.Add(parts.Count > 1 ? $"'''Part {WikiText.Roman(part.PartIndex)}'''<br>{body}" : body);
    }

    return string.Join("<br><br>", blocks);
  }

  // ── Row shapes (SQL aliases must match names and order) ─────────────────

  private sealed record DraftRow(
    Guid DraftId,
    string DraftPublicId,
    string Title,
    Guid? CampaignId,
    string? CampaignName
  );

  private sealed record PartRow(Guid PartId, Guid DraftId, int PartIndex);

  private sealed record EpisodeRow(Guid DraftId, int? EpisodeNumber);

  private sealed record ReleaseRow(Guid PartId, DateOnly ReleaseDate);

  private sealed record ParticipantRow(
    Guid ParticipantRowId,
    Guid PartId,
    Guid ParticipantValue,
    int ParticipantKind,
    string DisplayName
  );

  private sealed record HostRow(Guid PartId, int Role, string DisplayName);

  private sealed record TriviaRow(
    Guid PartId,
    int Position,
    int QuestionsWon,
    Guid ParticipantValue,
    int ParticipantKind
  );

  private sealed record CategoryRow(Guid DraftId, string Name);

  private sealed record TimelineRow(
    Guid DraftId,
    string Title,
    int PartIndex,
    int PartCount,
    DateOnly ReleaseDate,
    int? EpisodeNumber
  );

  private sealed record CampaignDraftRow(
    Guid DraftId,
    Guid CampaignId,
    string Title,
    DateOnly FirstRelease
  );

  private sealed record PredictionSetRow(
    Guid SetId,
    Guid PartId,
    string ContestantName,
    DateTime SubmittedAtUtc
  );

  private sealed record PredictionEntryRow(
    Guid SetId,
    string Title,
    bool? IsCorrect,
    int? OrderIndex
  );

  private sealed record PredictionModeRow(Guid PartId, int PredictionMode);

  private sealed record StandingRow(Guid SetId, int Points);

  private sealed class Data
  {
    public required Dictionary<Guid, List<PartRow>> PartsByDraft { get; init; }
    public required Dictionary<Guid, int?> EpisodeByDraft { get; init; }
    public required Dictionary<Guid, DateOnly> ReleaseByPart { get; init; }
    public required Dictionary<Guid, List<ParticipantRow>> ParticipantsByPart { get; init; }
    public required Dictionary<Guid, string> NameByRow { get; init; }
    public required Dictionary<Guid, List<HostRow>> HostsByPart { get; init; }
    public required Dictionary<Guid, List<PickRow>> PicksByPart { get; init; }
    public required Dictionary<Guid, List<VetoRow>> VetoesByPick { get; init; }
    public required Dictionary<Guid, List<TriviaRow>> TriviaByPart { get; init; }
    public required Dictionary<Guid, List<string>> CategoriesByDraft { get; init; }
    public required Dictionary<Guid, List<CampaignDraftRow>> CampaignDrafts { get; init; }
    public required Dictionary<Guid, List<PredictionSetRow>> SetsByPart { get; init; }
    public required Dictionary<Guid, int> ModeByPart { get; init; }
    public required Dictionary<Guid, List<PredictionEntryRow>> EntriesBySet { get; init; }
    public required Dictionary<Guid, int> PointsBySet { get; init; }
    public required List<TimelineRow> Timeline { get; init; }
  }
}
