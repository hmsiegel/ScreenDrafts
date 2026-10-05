namespace ScreenDrafts.Modules.Reporting.Features.Drafts.QueryStats;

/// <summary>
/// Loads every pick, credit and veto in scope for the custom query. The fact tables hold a few thousand rows,
/// so the query engine filters and groups in memory and the request never reaches the database as SQL.
/// Scope is the Record Book's: canonical picks, or all picks when includeAll is set.
/// </summary>
internal static class QueryDatasetLoader
{
  private const string PicksSql =
    RecordBookDataLoader.ScopedPicksCte
    + """

      SELECT
        sp.id                      AS PickId,
        sp.draft_id                AS DraftId,
        sp.draft_public_id         AS DraftPublicId,
        sp.draft_title             AS DraftTitle,
        sp.draft_type              AS DraftType,
        sp.series_name             AS SeriesName,
        (SELECT MIN(ds.episode_number)
         FROM reporting.draft_summaries ds
         WHERE ds.draft_id = sp.draft_id) AS EpisodeNumber,
        sp.media_public_id         AS MediaPublicId,
        sp.media_title             AS MediaTitle,
        (NOT sp.was_commissioner_overridden
          AND NOT (sp.was_vetoed AND NOT sp.was_veto_overridden)) AS Landed,
        (sp.was_vetoed AND NOT sp.was_veto_overridden)            AS VetoStanding,
        sp.was_commissioner_overridden                            AS CommissionerOverridden
      FROM scoped_picks sp
      """;

  private const string CreditsSql =
    RecordBookDataLoader.ScopedPicksCte
    + """

      SELECT
        c.pick_id            AS PickId,
        c.drafter_id_value   AS DrafterId,
        c.drafter_public_id  AS DrafterPublicId,
        c.drafter_name       AS DrafterName
      FROM reporting.pick_credit_facts c
      JOIN scoped_picks sp ON sp.id = c.pick_id
      """;

  private const string VetoesSql =
    RecordBookDataLoader.ScopedPicksCte
    + """

      SELECT
        v.id                     AS VetoId,
        v.pick_id                AS PickId,
        v.issued_by_kind         AS IssuedByKind,
        v.issued_by_id_value     AS IssuedByIdValue,
        v.issued_by_public_id    AS IssuedByPublicId,
        v.issued_by_name         AS IssuedByName,
        v.is_overridden          AS IsOverridden,
        v.is_self_veto           AS IsSelfVeto
      FROM reporting.veto_facts v
      JOIN scoped_picks sp ON sp.id = v.pick_id
      """;

  public static async Task<QueryDataset> LoadAsync(
    System.Data.Common.DbConnection connection,
    bool includeAll,
    CancellationToken cancellationToken
  )
  {
    ArgumentNullException.ThrowIfNull(connection);

    var parameters = new { IncludeAll = includeAll };

    var picks = (
      await connection.QueryAsync<QueryPickRow>(
        new CommandDefinition(PicksSql, parameters, cancellationToken: cancellationToken)
      )
    ).ToList();

    var credits = (
      await connection.QueryAsync<QueryCreditRow>(
        new CommandDefinition(CreditsSql, parameters, cancellationToken: cancellationToken)
      )
    ).ToList();

    var vetoes = (
      await connection.QueryAsync<QueryVetoRow>(
        new CommandDefinition(VetoesSql, parameters, cancellationToken: cancellationToken)
      )
    ).ToList();

    return new QueryDataset
    {
      Picks = picks,
      Credits = credits,
      Vetoes = vetoes,
    };
  }
}
