namespace ScreenDrafts.Modules.Reporting.Features.Drafters.UpdateDrafterHonorifics;

internal sealed class UpdateDrafterHonorificsCommandHandler(
  IDbConnectionFactory dbConnectionFactory,
  IDateTimeProvider dateTimeProvider,
  IEventBus eventBus)
  : ICommandHandler<UpdateDrafterHonorificsCommand>
{
  private readonly IDbConnectionFactory _dbConnectionFactory = dbConnectionFactory;
  private readonly IDateTimeProvider _dateTimeProvider = dateTimeProvider;
  private readonly IEventBus _eventBus = eventBus;

  public async Task<Result> Handle(
    UpdateDrafterHonorificsCommand request,
    CancellationToken cancellationToken)
  {
    await using var connection = await _dbConnectionFactory.OpenConnectionAsync(cancellationToken);

    var currentTime = _dateTimeProvider.UtcNow;

    const string appearanceSql =
      """
      INSERT INTO reporting.drafter_canonical_appearances
        (id, drafter_id_value, draft_part_public_id, has_main_feed_release, appeared_at)
      VALUES (@Id, @DrafterIdValue, @DraftPartPublicId, @HasMainFeedRelease, @AppearedAt)
      ON CONFLICT (drafter_id_value, draft_part_public_id) DO NOTHING;
      """;

    var inserted = await connection.ExecuteAsync(
      new CommandDefinition(
        appearanceSql,
        new
        {
          Id = Guid.NewGuid(),
          request.DrafterIdValue,
          request.DraftPartPublicId,
          request.HasMainFeedRelease,
          AppearedAt = currentTime
        },
        cancellationToken: cancellationToken));

    if (inserted == 0)
    {
      // This means the appearance already exists, so no need to update honorifics
      return Result.Success();
    }

    // count canoncial appearances for the drafter, exclude the current draft part
    // OnMainFeed: Only parts with a main feed releaes count
    const string countSql =
      """
      SELECT COUNT(*)
      FROM reporting.drafter_canonical_appearances dca
      WHERE dca.drafter_id_value = @DrafterIdValue
        AND (
          @CanonicalPolicyValue = 0
          OR (
            @CanonicalPolicyValue = 2
            AND dca.has_main_feed_release = true
          )
        );
      """;

    var appearanceCount = await connection.ExecuteScalarAsync<int>(
      new CommandDefinition(
        countSql,
        new
        {
          request.DrafterIdValue,
          request.CanonicalPolicyValue
        },
        cancellationToken: cancellationToken));


    // Read current honorific row
    const string currentSql =
      """
      SELECT honorific, appearance_count
      FROM reporting.drafter_honorifics
      WHERE drafter_id_value = @DrafterIdValue;
      """;

    var current = await connection.QuerySingleOrDefaultAsync<(int Honorific, int AppearanceCount)>(
      new CommandDefinition(
        currentSql,
        new { request.DrafterIdValue },
        cancellationToken: cancellationToken));

    var previousHonorific = DrafterHonorific.FromValue(current.Honorific);
    var newHonorific = DrafterHonorific.FromAppearanceCount(appearanceCount);

    // Upsert current row
    const string upsertSql =
      """
      INSERT INTO reporting.drafter_honorifics
        (id, drafter_id_value, honorific, appearance_count, update_at_utc)
      VALUES (@Id, @DrafterIdValue, @HonorificValue, @AppearanceCount, @UpdatedAt)
      ON CONFLICT (drafter_id_value) DO UPDATE
      SET honorific = EXCLUDED.honorific,
          appearance_count = EXCLUDED.appearance_count,
          update_at_utc = EXCLUDED.update_at_utc;
      """;

    await connection.ExecuteAsync(
      new CommandDefinition(
        upsertSql,
        new
        {
          Id = Guid.NewGuid(),
          request.DrafterIdValue,
          HonorificValue = newHonorific.Value,
          AppearanceCount = appearanceCount,
          UpdatedAt = currentTime
        },
        cancellationToken: cancellationToken));

    // Only write the history and fire the integration event if the honorific has changed
    if (newHonorific == previousHonorific)
    {
      return Result.Success();
    }

    const string historySql =
      """
      INSERT INTO reporting.drafters_honorifics_history
        (id, drafter_id_value, honorific, appearance_count, achieved_at)
      VALUES (@Id, @DrafterIdValue, @HonorificValue, @AppearanceCount, @AchievedAt);
      """;

    await connection.ExecuteAsync(
      new CommandDefinition(
        historySql,
        new
        {
          Id = Guid.NewGuid(),
          request.DrafterIdValue,
          HonorificValue = newHonorific.Value,
          AppearanceCount = appearanceCount,
          AchievedAt = _dateTimeProvider.UtcNow
        },
        cancellationToken: cancellationToken));

    await _eventBus.PublishAsync(
      new DrafterHonorificEarnedIntegrationEvent(
        id: Guid.NewGuid(),
        occurredOnUtc: currentTime,
        drafterIdValue: request.DrafterIdValue,
        draftPartPublicId: request.DraftPartPublicId,
        previousHonorificValue: previousHonorific.Value,
        newHonorificValue: newHonorific.Value,
        appearanceCount: appearanceCount),
      cancellationToken);

    return Result.Success();
  }
}
