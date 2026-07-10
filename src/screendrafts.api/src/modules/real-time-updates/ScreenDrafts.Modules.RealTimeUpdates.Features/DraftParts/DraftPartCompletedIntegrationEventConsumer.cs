namespace ScreenDrafts.Modules.RealTimeUpdates.Features.DraftParts;

// CROSS-SCHEMA READS: intentional, not a boundary violation. RealTimeUpdates
// has no domain tables and performs no writes outside its own outbox/inbox —
// it exists purely to read current state (drafts.*, reporting.*) and
// broadcast it over SignalR. Sanctioned exception to the "no cross-schema
// SQL" rule; real_time_updates_user has SELECT-only grants on both schemas.
// See _crossschema_grant_realtimeupdates_readonly.sql. Every query in this
// class and in GamePlayTokenQuery falls under this exception — SELECT only,
// never a write, to any table outside RealTimeUpdates' own schema.

internal sealed partial class DraftPartCompletedIntegrationEventConsumer(
  IHubContext<DraftHub> hubContext,
  ILogger<DraftPartCompletedIntegrationEventConsumer> logger,
  IDbConnectionFactory dbConnectionFactory,
  IOptions<RealTimeUpdatesDraftsOptions> options
) : IntegrationEventHandler<DraftPartCompletedIntegrationEvent>
{
  private readonly IHubContext<DraftHub> _hubContext = hubContext;
  private readonly ILogger<DraftPartCompletedIntegrationEventConsumer> _logger = logger;
  private readonly IDbConnectionFactory _dbConnectionFactory = dbConnectionFactory;
  private readonly RealTimeUpdatesDraftsOptions _options = options.Value;

  public override async Task Handle(
    DraftPartCompletedIntegrationEvent integrationEvent,
    CancellationToken cancellationToken = default
  )
  {
    LogDraftPartCompleted(_logger, integrationEvent.DraftPartPublicId);

    await using var connection = await _dbConnectionFactory.OpenConnectionAsync(cancellationToken);

    // ── Movie honorifics ───────────────────────────────────────────────────
    // For each landed pick, count how many canonical appearances the movie had
    // BEFORE this draft part. If priorCount >= 1, this part pushes it to >= 2
    // appearances, which is the first named honorific threshold (MarqueeOfFame).
    // We also catch movies already past named thresholds (e.g. 5 → 6 is still
    // a High Five shout-out per domain rules). Skip the query entirely for
    // Patreon drafts since those never count as canonical.

    var movieHonorifics = new List<PickHonorificRecord>();

    if (!integrationEvent.IsPatreon && integrationEvent.Picks.Count > 0)
    {
      const string moviePriorCountSql = """
        SELECT
          mcp.movie_public_id  AS MoviePublicId,
          COUNT(*)             AS PriorCount
        FROM reporting.movie_canonical_picks mcp
        WHERE mcp.movie_public_id = ANY(@MoviePublicIds)
          AND mcp.draft_part_public_id != @DraftPartPublicId
        GROUP BY mcp.movie_public_id;
        """;

      var moviePublicIds = integrationEvent.Picks.Select(p => p.MediaPublicId).Distinct().ToArray();

      var priorCounts = (
        await connection.QueryAsync<(string MoviePublicId, int PriorCount)>(
          new CommandDefinition(
            moviePriorCountSql,
            new { MoviePublicIds = moviePublicIds, integrationEvent.DraftPartPublicId },
            cancellationToken: cancellationToken
          )
        )
      ).ToDictionary(r => r.MoviePublicId, r => r.PriorCount);

      foreach (var pick in integrationEvent.Picks)
      {
        var prior = priorCounts.GetValueOrDefault(pick.MediaPublicId, 0);
        // Any pick that pushes the movie to >= 2 canonical appearances earns
        // a shout-out. FromAppearanceCount handles both named thresholds and
        // the >= 5 "High Five extended" case naturally.
        if (prior >= 1)
        {
          movieHonorifics.Add(
            new PickHonorificRecord(
              MediaPublicId: pick.MediaPublicId,
              MediaTitle: pick.MediaTitle,
              BoardPosition: pick.Position,
              PriorCount: prior,
              NewCount: prior + 1
            )
          );
        }
      }
    }

    // ── Drafter honorifics ─────────────────────────────────────────────────
    // Same logic: count prior canonical appearances per drafter, excluding
    // this draft part. A drafter earns a shout-out if prior >= 4 (pushing
    // them to >= 5, the AllStar threshold) or already past any threshold.

    var drafterHonorifics = new List<DrafterHonorificRecord>();

    if (!integrationEvent.IsPatreon && integrationEvent.ParticipantPublicIds.Count > 0)
    {
      // ParticipantPublicIds are person public IDs (pe_ prefix). The drafter
      // canonical appearances table stores DrafterIdValue as a Guid. We need
      // to join through the drafters table to resolve the mapping.
      const string drafterPriorCountSql = """
        SELECT
          dca.drafter_id_value  AS DrafterIdValue,
          COUNT(*)              AS PriorCount
        FROM reporting.drafter_canonical_appearances dca
        WHERE dca.draft_part_public_id != @DraftPartPublicId
        GROUP BY dca.drafter_id_value
        HAVING COUNT(*) >= 1;
        """;

      var drafterPriorCounts = (
        await connection.QueryAsync<(Guid DrafterIdValue, int PriorCount)>(
          new CommandDefinition(
            drafterPriorCountSql,
            new { integrationEvent.DraftPartPublicId },
            cancellationToken: cancellationToken
          )
        )
      ).ToDictionary(r => r.DrafterIdValue, r => r.PriorCount);

      // Check whether each drafter participated in this part by also having
      // an appearance row for THIS draft part.
      const string drafterThisPartSql = """
        SELECT drafter_id_value
        FROM reporting.drafter_canonical_appearances
        WHERE draft_part_public_id = @DraftPartPublicId;
        """;

      var draftersThisPart = (
        await connection.QueryAsync<Guid>(
          new CommandDefinition(
            drafterThisPartSql,
            new { integrationEvent.DraftPartPublicId },
            cancellationToken: cancellationToken
          )
        )
      ).ToHashSet();

      foreach (var drafterIdValue in draftersThisPart)
      {
        var prior = drafterPriorCounts.GetValueOrDefault(drafterIdValue, 0);
        if (prior >= 4)
        {
          drafterHonorifics.Add(
            new DrafterHonorificRecord(
              DrafterIdValue: drafterIdValue,
              PriorCount: prior,
              NewCount: prior + 1
            )
          );
        }
      }
    }

    // ── Predictions ─────────────────────────────────────────────────────────
    // Scoring runs synchronously off the same DraftPartCompletedDomainEvent,
    // in-process, before the outbox poller ever delivers this integration
    // event — so by the time this consumer runs, prediction_results rows
    // should already be committed. The INNER JOIN on prediction_results is
    // deliberate: if a race ever does slip a set through unscored, it's
    // silently omitted from the summary rather than crashing the broadcast.

    const string predictionSql = """
      SELECT
        s.season_id        AS SeasonId,
        c.display_name    AS ContestantDisplayName,
        r.correct_count    AS CorrectCount,
        r.shoot_the_moon   AS ShootsTheMoon,
        r.points_awarded   AS PointsAwarded,
        e.media_title      AS MediaTitle,
        e.order_index      AS OrderIndex,
        e.is_correct       AS IsCorrect
      FROM drafts.draft_prediction_sets s
      JOIN drafts.draft_parts dp           ON dp.id = s.draft_part_id
      JOIN drafts.prediction_contestants c ON c.id  = s.contestant_id
      JOIN drafts.prediction_results r     ON r.set_id = s.id
      LEFT JOIN drafts.prediction_entries e ON e.set_id = s.id
      WHERE dp.public_id = @DraftPartPublicId
      ORDER BY c.display_name, e.order_index;
      """;

    var predictionRows = (
      await connection.QueryAsync<PredictionRow>(
        new CommandDefinition(
          predictionSql,
          new { integrationEvent.DraftPartPublicId },
          cancellationToken: cancellationToken
        )
      )
    ).ToList();

    var predictions = predictionRows
      .GroupBy(r => r.ContestantDisplayName)
      .Select(g =>
      {
        var first = g.First();
        return new PredictionSummaryRecord(
          ContestantDisplayName: g.Key,
          CorrectCount: first.CorrectCount,
          ShootsTheMoon: first.ShootsTheMoon,
          PointsAwarded: first.PointsAwarded,
          Entries: g.Where(r => r.MediaTitle is not null)
            .OrderBy(r => r.OrderIndex)
            .Select(r => new PredictionEntryRecord(r.MediaTitle!, r.IsCorrect))
            .ToList()
        );
      })
      .ToList();

    // ── Season standings ────────────────────────────────────────────────────
    // Only fetch if this part actually had predictions — no sets means no
    // season to look up. Points come from prediction_standings (the
    // authoritative running total, already updated by scoring); carryover is
    // summed separately since a contestant can carry points in from a prior
    // season.

    var standings = new List<StandingRecord>();

    if (predictionRows.Count > 0)
    {
      var seasonId = predictionRows[0].SeasonId;

      // Computed directly from prediction_results rather than read from
      // prediction_standings. prediction_standings is updated by a separate
      // domain event (PredictionSetScoredDomainEvent) on a separate write
      // path from the one that writes prediction_results, and the two have
      // been observed out of sync at this exact moment (this consumer runs
      // right after scoring). Summing prediction_results directly avoids
      // depending on that second write having landed yet, since results
      // are written earlier in the same scoring command, before RaiseScored
      // is even called.
      const string standingsSql = """
        SELECT
          c.display_name    AS ContestantDisplayName,
          CAST(COALESCE(SUM(r.points_awarded), 0) AS INT4) AS Points,
          CAST(COALESCE(MAX(co.CarryoverPoints), 0) AS INT4) AS CarryoverPoints
        FROM drafts.draft_prediction_sets s
        JOIN drafts.prediction_contestants c ON c.id = s.contestant_id
        JOIN drafts.prediction_results r ON r.set_id = s.id
        JOIN drafts.peopln ppl ON ppl.id = c.person_id
        LEFT JOIN (
          SELECT contestant_id, SUM(points) AS CarryoverPoints
          FROM drafts.prediction_carryovers
          WHERE season_id = @SeasonId
          GROUP BY contestant_id
        ) co ON co.contestant_id = c.id
        WHERE s.season_id = @SeasonId
          AND ppl.public_id = ANY(@CommissionerPublicIds)
        GROUP BY c.display_name
        ORDER BY (SUM(r.points_awarded) + COALESCE(MAX(co.CarryoverPoints), 0)) DESC;
        """;

      var commissionerPublicIds = _options.CommissionerPersonPublicIds;

      standings = (
        await connection.QueryAsync<StandingRow>(
          new CommandDefinition(
            standingsSql,
            new { SeasonId = seasonId, CommissionerPublicIds = commissionerPublicIds },
            cancellationToken: cancellationToken
          )
        )
      )
        .Select(r => new StandingRecord(
          ContestantDisplayName: r.ContestantDisplayName,
          Points: r.Points,
          CarryoverPoints: r.CarryoverPoints,
          TotalPoints: r.Points + r.CarryoverPoints
        ))
        .ToList();
    }

    var payload = new
    {
      integrationEvent.DraftPartPublicId,
      integrationEvent.DraftId,
      integrationEvent.DraftPublicId,
      integrationEvent.Title,
      integrationEvent.DraftType,
      integrationEvent.PartIndex,
      integrationEvent.TotalParts,
      integrationEvent.TotalPicks,
      integrationEvent.VetoCount,
      integrationEvent.IsPatreon,
      Picks = integrationEvent.Picks.Select(p => new
      {
        p.Position,
        p.MediaPublicId,
        p.MediaTitle,
      }),
      MovieHonorifics = movieHonorifics,
      DrafterHonorifics = drafterHonorifics,
      Predictions = predictions,
      Standings = standings,
    };

    await _hubContext
      .Clients.Group(DraftHub.GroupName(integrationEvent.DraftPartPublicId))
      .SendAsync("PartCompleted", payload, cancellationToken);
  }

  [LoggerMessage(
    0,
    LogLevel.Information,
    "DraftPart {DraftPartPublicId} completed — broadcasting summary."
  )]
  private static partial void LogDraftPartCompleted(ILogger logger, string draftPartPublicId);
}

internal sealed record PickHonorificRecord(
  string MediaPublicId,
  string MediaTitle,
  int BoardPosition,
  int PriorCount,
  int NewCount
);

internal sealed record DrafterHonorificRecord(Guid DrafterIdValue, int PriorCount, int NewCount);

internal sealed record PredictionSummaryRecord(
  string ContestantDisplayName,
  int CorrectCount,
  bool ShootsTheMoon,
  int PointsAwarded,
  IReadOnlyList<PredictionEntryRecord> Entries
);

internal sealed record PredictionEntryRecord(string MediaTitle, bool? IsCorrect);

internal sealed record StandingRecord(
  string ContestantDisplayName,
  int Points,
  int CarryoverPoints,
  int TotalPoints
);

internal sealed record PredictionRow(
  Guid SeasonId,
  string ContestantDisplayName,
  int CorrectCount,
  bool ShootsTheMoon,
  int PointsAwarded,
  string? MediaTitle,
  int? OrderIndex,
  bool? IsCorrect
);

internal sealed record StandingRow(string ContestantDisplayName, int Points, int CarryoverPoints);
