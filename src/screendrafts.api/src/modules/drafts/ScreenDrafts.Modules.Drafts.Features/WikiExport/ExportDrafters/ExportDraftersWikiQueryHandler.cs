namespace ScreenDrafts.Modules.Drafts.Features.WikiExport.ExportDrafters;

// ── Handler ───────────────────────────────────────────────────────────────

internal sealed class ExportDraftersWikiQueryHandler(
  IDbConnectionFactory connectionFactory,
  IOptions<DraftsOptions> options,
  IReportingApi reportingApi
) : IQueryHandler<ExportDraftersWikiQuery, ExportWikiResponse>
{
  private const int MainFeedChannel = 0;

#pragma warning disable S1075
  private const string TwitterUrl = "https://twitter.com/{0}";
  private const string LetterboxdUrl = "https://letterboxd.com/{0}/";
  private const string InstagramUrl = "https://www.instagram.com/{0}/";
  private const string BlueskyUrl = "https://bsky.app/profile/{0}";
#pragma warning restore S1075

  private readonly IDbConnectionFactory _connectionFactory = connectionFactory;
  private readonly DraftsOptions _options = options.Value;
  private readonly IReportingApi _reportingApi = reportingApi;

  public async Task<Result<ExportWikiResponse>> Handle(
    ExportDraftersWikiQuery request,
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

    var requested = request.DrafterPublicIds.Distinct(StringComparer.Ordinal).ToList();

    var drafters = await ReadAsync<DrafterRow>(
      """
      SELECT
        d.id                  AS DrafterId,
        d.public_id           AS DrafterPublicId,
        pe.id                 AS PersonId,
        pe.public_id          AS PersonPublicId,
        pe.display_name       AS DisplayName,
        pe.first_name         AS FirstName,
        pe.last_name          AS LastName,
        pe.twitter_handle     AS TwitterHandle,
        pe.letterboxd_handle  AS LetterboxdHandle,
        pe.instagram_handle   AS InstagramHandle,
        pe.bluesky_handle     AS BlueskyHandle
      FROM drafts.drafters d
      JOIN drafts.people pe ON pe.id = d.person_id
      WHERE d.public_id = ANY(@PublicIds)
      """,
      new { PublicIds = requested.ToArray() }
    );

    if (drafters.Count == 0)
    {
      return Result.Failure<ExportWikiResponse>(WikiExportErrors.NoDraftersFound);
    }

    var drafterIds = drafters.Select(d => d.DrafterId).ToArray();
    var personIds = drafters.Select(d => d.PersonId).ToArray();

    // Main-feed appearances only.
    var participations = await ReadAsync<ParticipationRow>(
      """
      SELECT
        dpp.id                    AS ParticipantRowId,
        dpp.participant_id_value  AS DrafterId,
        dp.id                     AS PartId,
        dp.part_index             AS PartIndex,
        d.id                      AS DraftId,
        d.title                   AS DraftTitle,
        (
          SELECT COUNT(*)::int FROM drafts.draft_parts x WHERE x.draft_id = d.id
        )                         AS PartCount,
        dr.release_date           AS ReleaseDate,
        dpp.vetoes_used           AS VetoesUsed,
        (dpp.starting_vetoes + dpp.vetoes_rolling_in + dpp.awarded_vetoes - dpp.vetoes_used)
                                  AS VetoesLeft,
        (dpp.veto_overrides_rolling_in + dpp.awarded_veto_overrides - dpp.veto_overrides_used)
                                  AS OverridesLeft
      FROM drafts.draft_part_participants dpp
      JOIN drafts.draft_parts dp ON dp.id = dpp.draft_part_id
      JOIN drafts.drafts d       ON d.id = dp.draft_id
      JOIN drafts.draft_releases dr
        ON dr.part_id = dp.id
       AND dr.release_channel = @Channel
      WHERE dpp.participant_kind_value = 0
        AND dpp.participant_id_value = ANY(@DrafterIds)
        AND d.is_deleted = FALSE
      """,
      new { DrafterIds = drafterIds, Channel = MainFeedChannel }
    );

    var rowIds = participations.Select(p => p.ParticipantRowId).ToArray();

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
      WHERE pk.played_by_participant_id = ANY(@RowIds)
      """,
      new { RowIds = rowIds }
    );

    var pickIds = picks.Select(p => p.PickId).ToArray();

    var vetoesOnPicks = await ReadAsync<VetoRow>(
      """
      SELECT
        v.target_pick_id              AS PickId,
        v.sequence                    AS Sequence,
        v.issued_by_participant_id    AS IssuedByParticipantRowId,
        v.is_overridden               AS IsOverridden,
        vo.issued_by_participant_id   AS OverriddenByParticipantRowId
      FROM drafts.vetoes v
      LEFT JOIN drafts.veto_overrides vo ON vo.veto_id = v.id
      WHERE v.target_pick_id = ANY(@PickIds)
      """,
      new { PickIds = pickIds }
    );

    var issuedVetoes = await ReadAsync<IssuedVetoRow>(
      """
      SELECT
        v.issued_by_participant_id    AS IssuedByParticipantRowId,
        pk.id                         AS PickId,
        pk.position                   AS Position,
        pk.play_order                 AS PlayOrder,
        COALESCE(sd.index, 0)         AS SubDraftIndex,
        pk.movie_id                   AS MovieId,
        m.movie_title                 AS Title,
        m.year                        AS Year,
        pk.played_by_participant_id   AS PickedByParticipantRowId,
        v.is_overridden               AS IsOverridden,
        vo.issued_by_participant_id   AS OverriddenByParticipantRowId
      FROM drafts.vetoes v
      JOIN drafts.picks pk               ON pk.id = v.target_pick_id
      JOIN drafts.movies m               ON m.id = pk.movie_id
      LEFT JOIN drafts.sub_drafts sd     ON sd.id = pk.sub_draft_id
      LEFT JOIN drafts.veto_overrides vo ON vo.veto_id = v.id
      WHERE v.issued_by_participant_id = ANY(@RowIds)
      """,
      new { RowIds = rowIds }
    );

    // Names for everyone who vetoed, overrode, or was vetoed.
    var nameRowIds = vetoesOnPicks
      .SelectMany(v => new[] { (Guid?)v.IssuedByParticipantRowId, v.OverriddenByParticipantRowId })
      .Concat(
        issuedVetoes.SelectMany(v =>
          new[] { (Guid?)v.PickedByParticipantRowId, v.OverriddenByParticipantRowId }
        )
      )
      .Where(id => id.HasValue)
      .Select(id => id!.Value)
      .Distinct()
      .ToArray();

    var names = await ReadAsync<NameRow>(
      """
      SELECT
        dpp.id AS ParticipantRowId,
        COALESCE(pe.display_name, dt.name, @CommunityName) AS DisplayName
      FROM drafts.draft_part_participants dpp
      LEFT JOIN drafts.drafters dr       ON dr.id = dpp.participant_id_value
                                        AND dpp.participant_kind_value = 0
      LEFT JOIN drafts.people pe         ON pe.id = dr.person_id
      LEFT JOIN drafts.drafter_teams dt  ON dt.id = dpp.participant_id_value
                                        AND dpp.participant_kind_value = 1
      WHERE dpp.id = ANY(@Ids)
      """,
      new { Ids = nameRowIds, CommunityName = WikiText.PatreonMembers }
    );

    var hosting = await ReadAsync<HostingRow>(
      """
      SELECT
        h.person_id             AS PersonId,
        dh.role                 AS Role,
        d.title                 AS DraftTitle,
        MIN(dr.release_date)    AS FirstRelease
      FROM drafts.draft_hosts dh
      JOIN drafts.hosts h        ON h.id = dh.host_id
      JOIN drafts.draft_parts dp ON dp.id = dh.draft_part_id
      JOIN drafts.drafts d       ON d.id = dp.draft_id
      JOIN drafts.draft_releases dr
        ON dr.part_id = dp.id
       AND dr.release_channel = @Channel
      WHERE h.person_id = ANY(@PersonIds)
        AND d.is_deleted = FALSE
      GROUP BY h.person_id, dh.role, d.title
      ORDER BY FirstRelease
      """,
      new { PersonIds = personIds, Channel = MainFeedChannel }
    );

    var data = new Data
    {
      ParticipationsByDrafter = participations
        .GroupBy(p => p.DrafterId)
        .ToDictionary(g => g.Key, g => g.ToList()),
      PicksByRow = picks.GroupBy(p => p.ParticipantRowId).ToDictionary(g => g.Key, g => g.ToList()),
      IssuedByRow = issuedVetoes
        .GroupBy(v => v.IssuedByParticipantRowId)
        .ToDictionary(g => g.Key, g => g.ToList()),
      VetoesByPick = vetoesOnPicks
        .GroupBy(v => v.PickId)
        .ToDictionary(g => g.Key, g => g.OrderBy(v => v.Sequence).ToList()),
      NameByRow = names.ToDictionary(n => n.ParticipantRowId, n => n.DisplayName),
      HostingByPerson = hosting.GroupBy(h => h.PersonId).ToDictionary(g => g.Key, g => g.ToList()),
    };

    var pages = new List<WikiPage>();

    foreach (var drafter in drafters.OrderBy(d => requested.IndexOf(d.DrafterPublicId)))
    {
      var honorific = await _reportingApi.GetDrafterHonorificAsync(
        drafter.DrafterId,
        cancellationToken
      );

      pages.Add(
        new WikiPage(drafter.DisplayName, RenderDrafter(drafter, honorific?.HonorificName, data))
      );
    }

    return Result.Success(WikiText.ToResponse(pages, "drafters"));
  }

  // ── Rendering ───────────────────────────────────────────────────────────

  private string RenderDrafter(DrafterRow drafter, string? honorificName, Data data)
  {
    string NameOf(Guid? rowId) =>
      rowId is { } id && data.NameByRow.TryGetValue(id, out var name) ? name : "Unknown";

    var participations = data
      .ParticipationsByDrafter.GetValueOrDefault(drafter.DrafterId, [])
      .OrderBy(p => p.ReleaseDate)
      .ThenBy(p => p.DraftTitle, StringComparer.OrdinalIgnoreCase)
      .ThenBy(p => p.PartIndex)
      .ToList();

    var picks = participations
      .SelectMany(p => data.PicksByRow.GetValueOrDefault(p.ParticipantRowId, []))
      .ToList();

    // Same definitions as the drafter profile page, but main-feed drafts only (no Patreon).
    var totalDrafts = participations.Select(p => p.DraftId).Distinct().Count();
    var vetoesUsed = participations.Sum(p => p.VetoesUsed);

    var filmsDrafted = picks.Count(p =>
      !p.WasCommissionerOverride
      && !WikiText.IsFinallyVetoed(data.VetoesByPick.GetValueOrDefault(p.PickId, []))
    );

    var timesVetoed = picks.Sum(p =>
      data.VetoesByPick.GetValueOrDefault(p.PickId, []).Count(v => !v.IsOverridden)
    );

    var latest = participations.LastOrDefault();
    var hasRolloverVeto = latest is not null && latest.VetoesLeft >= 1;
    var hasRolloverVetoOverride = latest is not null && latest.OverridesLeft >= 1;

    var draftTitles = participations
      .Select(p => p.DraftTitle)
      .Distinct(StringComparer.OrdinalIgnoreCase)
      .ToList();

    var isCommissioner = _options.CommissionerPersonPublicIds.Contains(drafter.PersonPublicId);

    var hosting = data.HostingByPerson.GetValueOrDefault(drafter.PersonId, []);
    var primaryRole = ScreenDrafts.Modules.Drafts.Domain.Hosts.HostRole.Primary.Value;

    var guestCommissionerTitles = isCommissioner
      ? []
      : hosting.Where(h => h.Role == primaryRole).Select(h => h.DraftTitle).Distinct().ToList();

    var guestCoCommissionerTitles = isCommissioner
      ? []
      : hosting.Where(h => h.Role != primaryRole).Select(h => h.DraftTitle).Distinct().ToList();

    var sb = new StringBuilder();

    void Line(string text) => sb.Append(text).Append('\n');

    Line("{{Drafter");
    Line("  | image1=");
    Line(
      $"  | honor={(string.IsNullOrWhiteSpace(honorificName) ? string.Empty : $"{honorificName} Banner.jpg")}"
    );
    Line($"  | drafts={totalDrafts.ToString(CultureInfo.InvariantCulture)}");
    Line(
      $"  | first_draft={(participations.Count > 0 ? WikiText.Link(participations[0].DraftTitle) : string.Empty)}"
    );
    Line(
      $"  | last_draft={(latest is not null ? WikiText.Link(latest.DraftTitle) : string.Empty)}"
    );
    Line($"  | filmsdrafted={filmsDrafted.ToString(CultureInfo.InvariantCulture)}");
    Line($"  | picksvetoed={timesVetoed.ToString(CultureInfo.InvariantCulture)}");
    Line($"  | vetoused={vetoesUsed.ToString(CultureInfo.InvariantCulture)}");
    Line($"  | rollover={(hasRolloverVeto ? "Y" : string.Empty)}");
    Line($"  | override={(hasRolloverVetoOverride ? "Y" : string.Empty)}");
    Line($"  | twitter={Social(drafter.TwitterHandle, "Twitter", TwitterUrl)}");
    Line($"  | letterboxd={Social(drafter.LetterboxdHandle, "Letterboxd", LetterboxdUrl)}");
    Line($"  | instagram={Social(drafter.InstagramHandle, "Instagram", InstagramUrl)}");
    Line($"  | bluesky={Social(drafter.BlueskyHandle, "Bluesky", BlueskyUrl)}");
    sb.Append("}}");

    var role = isCommissioner ? "drafter" : "[[Guest G.M.]]";
    sb.Append(
      CultureInfo.InvariantCulture,
      $"'''{drafter.DisplayName}''' is a {role} on [[Screen Drafts]]"
    );

    if (draftTitles.Count > 0)
    {
      var linked = draftTitles.Select(WikiText.Link).ToList();
      sb.Append(
        CultureInfo.InvariantCulture,
        $", participating in the {WikiText.JoinNames(linked)}"
      );
    }

    sb.Append(".\n\n");

    var clauses = new List<string>();

    if (guestCommissionerTitles.Count > 0)
    {
      clauses.Add(
        $"[[Guest Commissioner]] for the {WikiText.JoinNames([.. guestCommissionerTitles.Select(WikiText.Link)])}"
      );
    }

    if (guestCoCommissionerTitles.Count > 0)
    {
      clauses.Add(
        $"[[Guest Co-Commissioner]] for the {WikiText.JoinNames([.. guestCoCommissionerTitles.Select(WikiText.Link)])}"
      );
    }

    if (clauses.Count > 0)
    {
      Line(
        $"{WikiText.FirstToken(drafter.DisplayName)} also served as {string.Join(" and ", clauses)}."
      );
      Line(string.Empty);
    }

    Line("==Draft History==");

    foreach (var year in participations.GroupBy(p => p.ReleaseDate.Year))
    {
      Line($"=={year.Key.ToString(CultureInfo.InvariantCulture)}==");

      foreach (var participation in year)
      {
        var heading =
          participation.PartCount > 1
            ? $"===[[{participation.DraftTitle}]] Part {WikiText.Roman(participation.PartIndex)}==="
            : $"===[[{participation.DraftTitle}]]===";

        Line(heading);

        var entries = new List<(int SubDraft, int PlayOrder, string Text)>();

        foreach (var pick in data.PicksByRow.GetValueOrDefault(participation.ParticipantRowId, []))
        {
          entries.Add(
            (
              pick.SubDraftIndex,
              pick.PlayOrder,
              PickBullet(pick, data.VetoesByPick.GetValueOrDefault(pick.PickId, []), NameOf)
            )
          );
        }

        foreach (var veto in data.IssuedByRow.GetValueOrDefault(participation.ParticipantRowId, []))
        {
          entries.Add((veto.SubDraftIndex, veto.PlayOrder, VetoBullet(veto, NameOf)));
        }

        foreach (var entry in entries.OrderBy(e => e.SubDraft).ThenBy(e => e.PlayOrder))
        {
          Line(entry.Text);
        }
      }
    }

    Line(string.Empty);

    if (
      !string.IsNullOrWhiteSpace(drafter.LastName) && !string.IsNullOrWhiteSpace(drafter.FirstName)
    )
    {
      Line($"{{{{DEFAULTSORT:{drafter.LastName}, {drafter.FirstName}}}}}");
    }

    Line("[[Category:Drafters]]");

    if (!string.IsNullOrWhiteSpace(honorificName))
    {
      Line($"[[Category:{honorificName}s]]");
    }

    if (guestCoCommissionerTitles.Count > 0)
    {
      Line("[[Category:Guest Co-Commissioner]]");
    }

    if (guestCommissionerTitles.Count > 0)
    {
      Line("[[Category:Guest Commissioner]]");
    }

    return sb.ToString();
  }

  private static string Social(string? handle, string icon, string urlFormat)
  {
    var clean = (handle ?? string.Empty).Trim().TrimStart('@');

    return clean.Length == 0
      ? string.Empty
      : $"[[File:{icon}.png|50px|alt={icon}|link={string.Format(CultureInfo.InvariantCulture, urlFormat, clean)}]]";
  }

  private static string PickBullet(
    PickRow pick,
    IReadOnlyList<VetoRow> vetoes,
    Func<Guid?, string> nameOf
  )
  {
    var film =
      $"{WikiText.Link(pick.Title)} at No. {pick.Position.ToString(CultureInfo.InvariantCulture)}";
    var chain = WikiText.OverrideChain(vetoes, nameOf);

    if (pick.WasCommissionerOverride)
    {
      return $"* <s>{film}</s> {WikiText.CommissionerOverrideNote}";
    }

    if (WikiText.IsFinallyVetoed(vetoes))
    {
      var prefix = chain.Length > 0 ? chain + " " : string.Empty;
      return $"* <s>{film}</s> {prefix}vetoed by {WikiText.Link(nameOf(vetoes[^1].IssuedByParticipantRowId))}";
    }

    return chain.Length > 0 ? $"* {film} {chain}" : $"* {film}";
  }

  private static string VetoBullet(IssuedVetoRow veto, Func<Guid?, string> nameOf)
  {
    var text =
      $"* Vetoed {WikiText.Link(veto.Title)} drafted by {WikiText.Link(nameOf(veto.PickedByParticipantRowId))} "
      + $"at No. {veto.Position.ToString(CultureInfo.InvariantCulture)}";

    return veto.IsOverridden
      ? $"{text} <!-- veto overridden by {nameOf(veto.OverriddenByParticipantRowId)} -->"
      : text;
  }

  // ── Row shapes (SQL aliases must match names and order) ─────────────────

  private sealed record DrafterRow(
    Guid DrafterId,
    string DrafterPublicId,
    Guid PersonId,
    string PersonPublicId,
    string DisplayName,
    string? FirstName,
    string? LastName,
    string? TwitterHandle,
    string? LetterboxdHandle,
    string? InstagramHandle,
    string? BlueskyHandle
  );

  private sealed record ParticipationRow(
    Guid ParticipantRowId,
    Guid DrafterId,
    Guid PartId,
    int PartIndex,
    Guid DraftId,
    string DraftTitle,
    int PartCount,
    DateOnly ReleaseDate,
    int VetoesUsed,
    int VetoesLeft,
    int OverridesLeft
  );

  private sealed record IssuedVetoRow(
    Guid IssuedByParticipantRowId,
    Guid PickId,
    int Position,
    int PlayOrder,
    int SubDraftIndex,
    Guid MovieId,
    string Title,
    string? Year,
    Guid PickedByParticipantRowId,
    bool IsOverridden,
    Guid? OverriddenByParticipantRowId
  );

  private sealed record NameRow(Guid ParticipantRowId, string DisplayName);

  private sealed record HostingRow(
    Guid PersonId,
    int Role,
    string DraftTitle,
    DateOnly FirstRelease
  );

  private sealed class Data
  {
    public required Dictionary<Guid, List<ParticipationRow>> ParticipationsByDrafter { get; init; }
    public required Dictionary<Guid, List<PickRow>> PicksByRow { get; init; }
    public required Dictionary<Guid, List<IssuedVetoRow>> IssuedByRow { get; init; }
    public required Dictionary<Guid, List<VetoRow>> VetoesByPick { get; init; }
    public required Dictionary<Guid, string> NameByRow { get; init; }
    public required Dictionary<Guid, List<HostingRow>> HostingByPerson { get; init; }
  }
}
