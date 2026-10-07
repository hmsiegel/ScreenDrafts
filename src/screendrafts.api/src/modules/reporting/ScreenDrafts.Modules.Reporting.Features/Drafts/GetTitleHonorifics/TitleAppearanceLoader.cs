namespace ScreenDrafts.Modules.Reporting.Features.Drafts.GetTitleHonorifics;

/// <summary>
/// Loads every canonical appearance of every title: one row per (title, draft part), taken from the first landed
/// pick of that title in the part. The release date is the PART's own main-feed release date, because the parts of a
/// multi-part draft come out on different days. Scope is the Record Book's. Landed means not removed by the commissioner and
/// not vetoed with the veto standing.
/// </summary>
internal static class TitleAppearanceLoader
{
  private const string AppearancesSql = RecordBookDataLoader.ScopedPicksCte + """
    ,
    part_order AS (
      SELECT
        r.draft_part_public_id,
        ROW_NUMBER() OVER (
          ORDER BY MIN(r.release_date), MIN(ds.episode_number), MIN(ds.part_index), r.draft_part_public_id
        )::int AS release_rank
      FROM reporting.draft_part_releases r
      LEFT JOIN reporting.draft_summaries ds ON ds.draft_part_public_id = r.draft_part_public_id
      WHERE r.release_channel = 'MainFeed'
      GROUP BY r.draft_part_public_id
    )
    SELECT DISTINCT ON (sp.media_public_id, sp.draft_part_public_id)
      sp.media_public_id                 AS MediaPublicId,
      sp.media_title                     AS MediaTitle,
      sp.draft_id                        AS DraftId,
      sp.draft_public_id                 AS DraftPublicId,
      sp.draft_title                     AS DraftTitle,
      sp.draft_part_public_id            AS PartPublicId,
      sp.part_index                      AS PartIndex,
      COALESCE(sp.sub_draft_index, 0)    AS SubDraftIndex,
      sp.position                        AS Position,
      sp.play_order                      AS PlayOrder,
      COALESCE(
        (SELECT MIN(ds.episode_number)
         FROM reporting.draft_summaries ds
         WHERE ds.draft_part_public_id = sp.draft_part_public_id),
        (SELECT MIN(ds.episode_number)
         FROM reporting.draft_summaries ds
         WHERE ds.draft_id = sp.draft_id))  AS EpisodeNumber,
      (SELECT TO_CHAR(MIN(r.release_date), 'YYYY-MM-DD')
       FROM reporting.draft_part_releases r
       WHERE r.draft_part_public_id = sp.draft_part_public_id
         AND r.release_channel = 'MainFeed') AS ReleasedOn,
      COALESCE(
        (SELECT MAX(ds.total_parts)::int FROM reporting.draft_summaries ds WHERE ds.draft_id = sp.draft_id),
        1)                               AS TotalParts,
      po.release_rank                    AS ReleaseRank
    FROM scoped_picks sp
    LEFT JOIN part_order po ON po.draft_part_public_id = sp.draft_part_public_id
    WHERE NOT sp.was_commissioner_overridden
      AND NOT (sp.was_vetoed AND NOT sp.was_veto_overridden)
    ORDER BY sp.media_public_id, sp.draft_part_public_id, sp.play_order
    """;

  public static async Task<IReadOnlyList<TitleAppearanceRow>> LoadAsync(
    System.Data.Common.DbConnection connection,
    bool includeAll,
    CancellationToken cancellationToken)
  {
    ArgumentNullException.ThrowIfNull(connection);

    var rows = await connection.QueryAsync<TitleAppearanceRow>(
      new CommandDefinition(
        AppearancesSql,
        new { IncludeAll = includeAll },
        cancellationToken: cancellationToken));

    return [.. rows];
  }
}
