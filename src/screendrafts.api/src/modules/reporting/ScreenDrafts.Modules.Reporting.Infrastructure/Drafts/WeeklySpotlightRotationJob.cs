namespace ScreenDrafts.Modules.Reporting.Infrastructure.Drafts;

[DisallowConcurrentExecution]
internal sealed partial class WeeklySpotlightRotationJob(
  IDbConnectionFactory dbConnectionFactory,
  ICacheService cacheService,
  ILogger<WeeklySpotlightRotationJob> logger,
  IDateTimeProvider dateTimeProvider
) : IJob
{
  private readonly IDbConnectionFactory _dbConnectionFactory = dbConnectionFactory;
  private readonly ICacheService _cacheService = cacheService;
  private readonly ILogger<WeeklySpotlightRotationJob> _logger = logger;
  private readonly IDateTimeProvider _dateTimeProvider = dateTimeProvider;

  private const string SpotlightCacheKey = "reporting:spotlight:active";

  public async Task Execute(IJobExecutionContext context)
  {
    var ct = context.CancellationToken;

    await using var connection = await _dbConnectionFactory.OpenConnectionAsync(ct);
    await using var transaction = await connection.BeginTransactionAsync(ct);

    try
    {
      // 1. Check if the current active spotlight is pinned. Skip is so.
      const string pinneedCheckSql = """
        SELECT id, is_pinned, created_at_utc
        FROM reporting.draft_spotlights
        WHERE is_active = true
        LIMIT 1
        """;

      var current = await connection.QuerySingleOrDefaultAsync<SpotlightRow>(
        new CommandDefinition(pinneedCheckSql, transaction: transaction, cancellationToken: ct)
      );

      if (current?.IsPinned is true)
      {
        LogRotationSkipped(_logger, current.Id);
        return;
      }

      // 2. Pick a random eligible inactive spotlight
      // Eligible = inactive, draft complete, draft is MainFeed
      const string randomCandidateSql = """
        SELECT s.id, s.is_pinned
        FROM reporting.draft_spotlights s
        JOIN reporting.draft_summaries ds ON ds.draft_public_id = s.draft_public_id
        WHERE s.is_active = false
          AND ds.is_complete = true
          AND ds.is_patreon = false
        ORDER BY random()
        LIMIT 1
        """;

      var next = await connection.QuerySingleOrDefaultAsync<SpotlightRow>(
        new CommandDefinition(randomCandidateSql, transaction: transaction, cancellationToken: ct)
      );

      if (next is null)
      {
        LogRotationSkippedNoEligible(_logger);
        return;
      }

      // 3. Deactivate current the activate next within the same transaction
      // Deactivate first to satisfy the partial unique index on is_active column

      if (current is not null)
      {
        const string deactivateSql = """
          UPDATE reporting.draft_spotlights
          SET is_active = false
          WHERE id = @Id
          """;
        await connection.ExecuteAsync(
          new CommandDefinition(
            deactivateSql,
            new { current.Id },
            transaction: transaction,
            cancellationToken: ct
          )
        );
      }

      const string activateSql = """
        UPDATE reporting.draft_spotlights
        SET is_active = true, activated_at_utc = @ActivatedAtUtc
        WHERE id = @Id
        """;

      var activatedAtUtc = _dateTimeProvider.UtcNow;

      await connection.ExecuteAsync(
        new CommandDefinition(
          activateSql,
          new { next.Id, ActivatedAtUtc = activatedAtUtc },
          transaction: transaction,
          cancellationToken: ct
        )
      );

      await transaction.CommitAsync(ct);

      // 4. Update cache
      await _cacheService.RemoveAsync(SpotlightCacheKey, ct);

      LogRotationCompleted(_logger, current?.Id ?? Guid.Empty, next.Id, activatedAtUtc);
    }
    catch (Exception ex)
    {
      await transaction.RollbackAsync(ct);

      LogRotationError(_logger, ex.Message);

      throw new JobExecutionException(ex, refireImmediately: false);
    }
  }

  private sealed record SpotlightRow(Guid Id, bool IsPinned, DateTime CreatedAtUtc);

  [LoggerMessage(
    EventId = 1,
    Level = LogLevel.Error,
    Message = "An error occurred while rotating the spotlight. Transaction rolled back. {ExceptionMessage}"
  )]
  private static partial void LogRotationError(ILogger logger, string exceptionMessage);

  [LoggerMessage(
    EventId = 2,
    Level = LogLevel.Information,
    Message = "Spotlight rotation skipped. Current spotlight {SpotlightId} is pinned."
  )]
  private static partial void LogRotationSkipped(ILogger logger, Guid spotlightId);

  [LoggerMessage(
    EventId = 3,
    Level = LogLevel.Warning,
    Message = "Spotlight rotation skipped. No eligible MainFeed spotlighs found in the pool."
  )]
  private static partial void LogRotationSkippedNoEligible(ILogger logger);

  [LoggerMessage(
    EventId = 4,
    Level = LogLevel.Information,
    Message = "Spotlight rotation completed. Spotlight rotated from {OldSpotlightId} to {NewSpotlightId} at {ActivatedAtUtc}."
  )]
  private static partial void LogRotationCompleted(
    ILogger logger,
    Guid oldSpotlightId,
    Guid newSpotlightId,
    DateTime activatedAtUtc
  );
}
