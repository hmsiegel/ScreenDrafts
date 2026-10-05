namespace ScreenDrafts.Modules.Reporting.Features.Drafts.GetRecordBook;

/// <summary>
/// Reads the Record Book source rows from the Reporting fact tables.
/// Scope: canonical picks (policy 0, or policy 2 with a main-feed release) unless includeAll is set,
/// which adds Patreon and Speed drafts. Canonical-ness is resolved here, at read time, because a
/// policy 2 draft can gain its main-feed release after the part completes.
/// "Landed" = not removed by the commissioner and not vetoed with the veto standing.
/// </summary>
internal static class RecordBookDataLoader
{
  internal const string ScopedPicksCte = """
    WITH scoped_picks AS (
      SELECT pf.*
      FROM reporting.pick_facts pf
      WHERE @IncludeAll
         OR pf.canonical_policy = 0
         OR (
           pf.canonical_policy = 2
           AND EXISTS (
             SELECT 1
             FROM reporting.draft_part_releases r
             WHERE r.draft_part_public_id = pf.draft_part_public_id
               AND r.release_channel = 'MainFeed'
           )
         )
    )
    """;

  private const string CreditedPicksSql =
    ScopedPicksCte
    + """

      SELECT
        c.drafter_id_value              AS DrafterId,
        MAX(c.drafter_public_id)        AS DrafterPublicId,
        MAX(c.drafter_person_public_id) AS DrafterPersonPublicId,
        MAX(c.drafter_name)             AS DrafterName,
        sp.draft_id                     AS DraftId,
        MAX(sp.draft_public_id)         AS DraftPublicId,
        MAX(sp.draft_title)             AS DraftTitle,
        (SELECT MIN(ds.episode_number)
         FROM reporting.draft_summaries ds
         WHERE ds.draft_id = sp.draft_id) AS EpisodeNumber,
        COUNT(*)::int                   AS PicksPlayed,
        COUNT(*) FILTER (
          WHERE NOT sp.was_commissioner_overridden
            AND NOT (sp.was_vetoed AND NOT sp.was_veto_overridden)
        )::int                          AS PicksLanded,
        COUNT(*) FILTER (
          WHERE sp.was_vetoed AND NOT sp.was_veto_overridden
        )::int                          AS PicksVetoed,
        COUNT(*) FILTER (
          WHERE sp.was_vetoed AND sp.was_veto_overridden
        )::int                          AS PicksSaved,
        COUNT(*) FILTER (
          WHERE sp.was_commissioner_overridden
        )::int                          AS PicksRemovedByCommissioner,
        COUNT(*) FILTER (
          WHERE sp.position = 1
            AND NOT sp.was_commissioner_overridden
            AND NOT (sp.was_vetoed AND NOT sp.was_veto_overridden)
        )::int                          AS No1Landed,
        COUNT(*) FILTER (
          WHERE sp.position = 1
            AND sp.was_vetoed AND NOT sp.was_veto_overridden
        )::int                          AS No1Vetoed
      FROM scoped_picks sp
      JOIN reporting.pick_credit_facts c ON c.pick_id = sp.id
      GROUP BY c.drafter_id_value, sp.draft_id
      """;

  private const string VetoesIssuedSql =
    ScopedPicksCte
    + """

      SELECT
        v.issued_by_id_value            AS DrafterId,
        MAX(v.issued_by_public_id)      AS DrafterPublicId,
        MAX(v.issued_by_name)           AS DrafterName,
        sp.draft_id                     AS DraftId,
        MAX(sp.draft_public_id)         AS DraftPublicId,
        MAX(sp.draft_title)             AS DraftTitle,
        COUNT(*)::int                   AS VetoesUsed,
        COUNT(*) FILTER (WHERE v.is_overridden)::int                          AS VetoesOverridden,
        COUNT(*) FILTER (WHERE v.is_self_veto AND NOT v.is_overridden)::int   AS SelfVetoes,
        COUNT(*) FILTER (WHERE sp.position = 1)::int                          AS No1VetoesUsed
      FROM reporting.veto_facts v
      JOIN scoped_picks sp ON sp.id = v.pick_id
      WHERE v.issued_by_kind = 0
      GROUP BY v.issued_by_id_value, sp.draft_id
      """;

  private const string OverridesDeployedSql =
    ScopedPicksCte
    + """

      SELECT
        v.overridden_by_id_value        AS DrafterId,
        MAX(v.overridden_by_public_id)  AS DrafterPublicId,
        MAX(v.overridden_by_name)       AS DrafterName,
        sp.draft_id                     AS DraftId,
        MAX(sp.draft_public_id)         AS DraftPublicId,
        MAX(sp.draft_title)             AS DraftTitle,
        COUNT(*)::int                   AS OverridesDeployed
      FROM reporting.veto_facts v
      JOIN scoped_picks sp ON sp.id = v.pick_id
      WHERE v.is_overridden = true
        AND v.overridden_by_kind = 0
      GROUP BY v.overridden_by_id_value, sp.draft_id
      """;

  private const string PartsSql =
    ScopedPicksCte
    + """

      SELECT
        sp.draft_id                       AS DraftId,
        MAX(sp.draft_public_id)           AS DraftPublicId,
        MAX(sp.draft_title)               AS DraftTitle,
        MAX(sp.draft_type)                AS DraftType,
        sp.draft_part_public_id           AS PartPublicId,
        MAX(sp.part_index)                AS PartIndex,
        COUNT(*)::int                     AS PicksPlayed,
        COUNT(*) FILTER (
          WHERE NOT sp.was_commissioner_overridden
            AND NOT (sp.was_vetoed AND NOT sp.was_veto_overridden)
        )::int                            AS PicksLanded,
        COUNT(DISTINCT sp.media_public_id)::int AS UniqueTitlesPlayed,
        COUNT(*) FILTER (
          WHERE sp.was_vetoed AND NOT sp.was_veto_overridden
        )::int                            AS PicksVetoed,
        COUNT(*) FILTER (
          WHERE sp.position = 1
            AND sp.was_vetoed AND NOT sp.was_veto_overridden
        )::int                            AS No1Vetoed,
        COUNT(*) FILTER (WHERE sp.was_commissioner_overridden)::int AS CommissionerOverrides,
        COALESCE(SUM(vf.vetoes), 0)::int     AS VetoesIssued,
        COALESCE(SUM(vf.overridden), 0)::int AS VetoesOverridden
      FROM scoped_picks sp
      LEFT JOIN (
        SELECT
          pick_id,
          COUNT(*)                                  AS vetoes,
          COUNT(*) FILTER (WHERE is_overridden)     AS overridden
        FROM reporting.veto_facts
        GROUP BY pick_id
      ) vf ON vf.pick_id = sp.id
      GROUP BY sp.draft_id, sp.draft_part_public_id
      """;

  private const string DraftTitlesSql =
    ScopedPicksCte
    + """

      SELECT
        sp.draft_id                             AS DraftId,
        COUNT(DISTINCT sp.media_public_id)::int AS UniqueTitlesPlayed
      FROM scoped_picks sp
      GROUP BY sp.draft_id
      """;

  private const string MediaSql =
    ScopedPicksCte
    + """

      SELECT
        sp.media_public_id                AS MediaPublicId,
        MAX(sp.media_title)               AS MediaTitle,
        COUNT(*) FILTER (
          WHERE NOT sp.was_commissioner_overridden
            AND NOT (sp.was_vetoed AND NOT sp.was_veto_overridden)
        )::int                            AS TimesDrafted,
        COUNT(*) FILTER (
          WHERE sp.position = 1
            AND NOT sp.was_commissioner_overridden
            AND NOT (sp.was_vetoed AND NOT sp.was_veto_overridden)
        )::int                            AS TimesDraftedNo1
      FROM scoped_picks sp
      GROUP BY sp.media_public_id
      HAVING COUNT(*) FILTER (
        WHERE NOT sp.was_commissioner_overridden
          AND NOT (sp.was_vetoed AND NOT sp.was_veto_overridden)
      ) > 0
      """;

  private const string PickSlotsSql =
    ScopedPicksCte
    + """

      SELECT
        c.drafter_id_value              AS DrafterId,
        MAX(c.drafter_public_id)        AS DrafterPublicId,
        MAX(c.drafter_person_public_id) AS DrafterPersonPublicId,
        MAX(c.drafter_name)             AS DrafterName,
        sp.draft_id                     AS DraftId,
        MAX(sp.draft_public_id)         AS DraftPublicId,
        MAX(sp.draft_title)             AS DraftTitle,
        sp.draft_part_public_id         AS PartPublicId,
        sp.sub_draft_index              AS SubDraftIndex,
        sp.position                     AS Position,
        COUNT(*) FILTER (
          WHERE sp.was_vetoed AND NOT sp.was_veto_overridden
        )::int                          AS TimesVetoed
      FROM scoped_picks sp
      JOIN reporting.pick_credit_facts c ON c.pick_id = sp.id
      GROUP BY c.drafter_id_value, sp.draft_id, sp.draft_part_public_id, sp.sub_draft_index, sp.position
      HAVING COUNT(*) FILTER (WHERE sp.was_vetoed AND NOT sp.was_veto_overridden) > 0
      """;

  private const string VetoTotalsSql =
    ScopedPicksCte
    + """

      SELECT
        COUNT(*) FILTER (WHERE NOT v.is_overridden)::int                         AS VetoesStood,
        COUNT(*) FILTER (WHERE v.is_overridden)::int                             AS VetoesOverridden,
        COUNT(*) FILTER (WHERE v.is_self_veto AND NOT v.is_overridden)::int      AS SelfVetoesStood
      FROM reporting.veto_facts v
      JOIN scoped_picks sp ON sp.id = v.pick_id
      """;

  // Title honorifics are always canonical, so this ignores the scope.
  private const string TitleHonorificTotalsSql = """
    SELECT
      COUNT(*) FILTER (WHERE appearance_honorific >= 1)::int AS MarqueeOfFame,
      COUNT(*) FILTER (WHERE appearance_honorific >= 2)::int AS HatTrick,
      COUNT(*) FILTER (WHERE appearance_honorific >= 3)::int AS GrandSlam
    FROM reporting.movie_honorifics
    """;

  public static async Task<RecordBookData> LoadAsync(
    System.Data.Common.DbConnection connection,
    bool includeAll,
    CancellationToken cancellationToken
  )
  {
    ArgumentNullException.ThrowIfNull(connection);

    var parameters = new { IncludeAll = includeAll };

    var credited = (
      await connection.QueryAsync<DrafterDraftRow>(
        new CommandDefinition(CreditedPicksSql, parameters, cancellationToken: cancellationToken)
      )
    ).ToList();

    foreach (var row in credited)
    {
      row.Appeared = true;
    }

    var rows = credited.ToDictionary(r => (r.DrafterId, r.DraftId));

    var issued = await connection.QueryAsync<DrafterDraftRow>(
      new CommandDefinition(VetoesIssuedSql, parameters, cancellationToken: cancellationToken)
    );

    foreach (var veto in issued)
    {
      var row = GetOrAdd(rows, veto);
      row.VetoesUsed = veto.VetoesUsed;
      row.VetoesOverridden = veto.VetoesOverridden;
      row.SelfVetoes = veto.SelfVetoes;
      row.No1VetoesUsed = veto.No1VetoesUsed;
    }

    var overrides = await connection.QueryAsync<DrafterDraftRow>(
      new CommandDefinition(OverridesDeployedSql, parameters, cancellationToken: cancellationToken)
    );

    foreach (var overrideRow in overrides)
    {
      GetOrAdd(rows, overrideRow).OverridesDeployed = overrideRow.OverridesDeployed;
    }

    var parts = (
      await connection.QueryAsync<DraftPartRow>(
        new CommandDefinition(PartsSql, parameters, cancellationToken: cancellationToken)
      )
    ).ToList();

    var uniqueTitlesByDraft = (
      await connection.QueryAsync<DraftTitleCountRow>(
        new CommandDefinition(DraftTitlesSql, parameters, cancellationToken: cancellationToken)
      )
    ).ToDictionary(r => r.DraftId, r => r.UniqueTitlesPlayed);

    var media = (
      await connection.QueryAsync<MediaRow>(
        new CommandDefinition(MediaSql, parameters, cancellationToken: cancellationToken)
      )
    ).ToList();

    var pickSlots = (
      await connection.QueryAsync<PickSlotRow>(
        new CommandDefinition(PickSlotsSql, parameters, cancellationToken: cancellationToken)
      )
    ).ToList();

    var vetoTotals = await connection.QuerySingleAsync<VetoTotalsRow>(
      new CommandDefinition(VetoTotalsSql, parameters, cancellationToken: cancellationToken)
    );

    var honorificTotals = await connection.QuerySingleAsync<TitleHonorificTotalsRow>(
      new CommandDefinition(TitleHonorificTotalsSql, cancellationToken: cancellationToken)
    );

    return new RecordBookData
    {
      DrafterDrafts = [.. rows.Values],
      Parts = parts,
      UniqueTitlesPlayedByDraft = uniqueTitlesByDraft,
      Media = media,
      PickSlots = pickSlots,
      VetoesStood = vetoTotals.VetoesStood,
      VetoesOverridden = vetoTotals.VetoesOverridden,
      SelfVetoesStood = vetoTotals.SelfVetoesStood,
      MarqueeOfFameTitles = honorificTotals.MarqueeOfFame,
      HatTrickTitles = honorificTotals.HatTrick,
      GrandSlamTitles = honorificTotals.GrandSlam,
    };
  }

  private static DrafterDraftRow GetOrAdd(
    Dictionary<(Guid DrafterId, Guid DraftId), DrafterDraftRow> rows,
    DrafterDraftRow source
  )
  {
    var key = (source.DrafterId, source.DraftId);

    if (rows.TryGetValue(key, out var existing))
    {
      return existing;
    }

    // The drafter issued a veto or an override in a draft where they hold no credited pick.
    // Keep the numbers, but Appeared stays false so it never counts as an appearance.
    source.Appeared = false;
    rows[key] = source;
    return source;
  }

  private sealed record DraftTitleCountRow(Guid DraftId, int UniqueTitlesPlayed);

  private sealed record VetoTotalsRow(int VetoesStood, int VetoesOverridden, int SelfVetoesStood);

  private sealed record TitleHonorificTotalsRow(int MarqueeOfFame, int HatTrick, int GrandSlam);
}
