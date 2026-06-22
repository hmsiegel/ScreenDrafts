namespace ScreenDrafts.Modules.RealTimeUpdates.Features.DraftParts;

internal sealed partial class DraftPartCompletedIntegrationEventConsumer(
  IHubContext<DraftHub> hubContext,
  ILogger<DraftPartCompletedIntegrationEventConsumer> logger,
  IDbConnectionFactory dbConnectionFactory
) : IntegrationEventHandler<DraftPartCompletedIntegrationEvent>
{
  private readonly IHubContext<DraftHub> _hubContext = hubContext;
  private readonly ILogger<DraftPartCompletedIntegrationEventConsumer> _logger = logger;
  private readonly IDbConnectionFactory _dbConnectionFactory = dbConnectionFactory;

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
