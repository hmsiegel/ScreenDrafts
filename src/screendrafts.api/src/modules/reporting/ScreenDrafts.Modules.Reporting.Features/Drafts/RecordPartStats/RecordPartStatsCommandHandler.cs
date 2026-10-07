namespace ScreenDrafts.Modules.Reporting.Features.Drafts.RecordPartStats;

/// <summary>
/// Replaces every fact row for the draft part inside one transaction, so re-completing a part
/// (or receiving the event twice) leaves exactly one consistent set of rows.
/// </summary>
internal sealed class RecordPartStatsCommandHandler(
  IDbConnectionFactory connectionFactory,
  ICacheService cacheService,
  IDateTimeProvider dateTimeProvider
) : ICommandHandler<RecordPartStatsCommand>
{
  private readonly IDbConnectionFactory _connectionFactory = connectionFactory;
  private readonly ICacheService _cacheService = cacheService;
  private readonly IDateTimeProvider _dateTimeProvider = dateTimeProvider;

  public async Task<Result> Handle(
    RecordPartStatsCommand request,
    CancellationToken cancellationToken
  )
  {
    var stats = request.Stats;
    var recordedAtUtc = _dateTimeProvider.UtcNow;

    var pickRows = stats
      .Picks.Select(p => new
      {
        Id = p.PickId,
        stats.DraftId,
        stats.DraftPublicId,
        stats.DraftPartPublicId,
        stats.PartIndex,
        stats.DraftTitle,
        stats.DraftType,
        stats.SeriesName,
        CanonicalPolicy = stats.CanonicalPolicyValue,
        p.SubDraftIndex,
        p.Position,
        p.PlayOrder,
        p.MediaPublicId,
        p.MediaTitle,
        p.PlayedByKind,
        p.PlayedByIdValue,
        p.PlayedByPublicId,
        p.PlayedByName,
        VetoCount = p.Vetoes.Count,
        WasVetoed = p.Vetoes.Count > 0,
        WasVetoOverridden = p.Vetoes.Count > 0 && p.Vetoes.MaxBy(v => v.Sequence)!.IsOverridden,
        WasCommissionerOverridden = p.IsCommissionerOverridden,
        RecordedAtUtc = recordedAtUtc,
      })
      .ToList();

    var vetoRows = stats
      .Picks.SelectMany(p =>
        p.Vetoes.Select(v => new
        {
          Id = v.VetoId,
          p.PickId,
          stats.DraftId,
          stats.DraftPartPublicId,
          v.Sequence,
          v.IssuedByKind,
          v.IssuedByIdValue,
          v.IssuedByPublicId,
          v.IssuedByName,
          v.IsOverridden,
          v.OverriddenByKind,
          v.OverriddenByIdValue,
          v.OverriddenByPublicId,
          v.OverriddenByName,
          IsSelfVeto = v.IssuedByKind == p.PlayedByKind && v.IssuedByIdValue == p.PlayedByIdValue,
          RecordedAtUtc = recordedAtUtc,
        })
      )
      .ToList();

    var creditRows = stats
      .Picks.SelectMany(p =>
        p.Credits.Select(c => new
        {
          Id = Guid.NewGuid(),
          p.PickId,
          stats.DraftId,
          stats.DraftPartPublicId,
          c.DrafterIdValue,
          c.DrafterPublicId,
          DrafterPersonPublicId = c.PersonPublicId ?? string.Empty,
          c.DrafterName,
          RecordedAtUtc = recordedAtUtc,
        })
      )
      .DistinctBy(c => (c.PickId, c.DrafterIdValue))
      .ToList();

    await using var connection = await _connectionFactory.OpenConnectionAsync(cancellationToken);
    await using var transaction = await connection.BeginTransactionAsync(cancellationToken);

    const string deleteSql = """
      DELETE FROM reporting.pick_credit_facts WHERE draft_part_public_id = @DraftPartPublicId;
      DELETE FROM reporting.veto_facts        WHERE draft_part_public_id = @DraftPartPublicId;
      DELETE FROM reporting.pick_facts        WHERE draft_part_public_id = @DraftPartPublicId;
      """;

    await connection.ExecuteAsync(
      new CommandDefinition(
        deleteSql,
        new { stats.DraftPartPublicId },
        transaction: transaction,
        cancellationToken: cancellationToken
      )
    );

    const string insertPickSql = """
      INSERT INTO reporting.pick_facts
        (id, draft_id, draft_public_id, draft_part_public_id, part_index, draft_title,
         draft_type, series_name, canonical_policy, sub_draft_index, position, play_order,
         media_public_id, media_title, played_by_kind, played_by_id_value,
         played_by_public_id, played_by_name, veto_count, was_vetoed,
         was_veto_overridden, was_commissioner_overridden, recorded_at_utc)
      VALUES
        (@Id, @DraftId, @DraftPublicId, @DraftPartPublicId, @PartIndex, @DraftTitle,
         @DraftType, @SeriesName, @CanonicalPolicy, @SubDraftIndex, @Position, @PlayOrder,
         @MediaPublicId, @MediaTitle, @PlayedByKind, @PlayedByIdValue,
         @PlayedByPublicId, @PlayedByName, @VetoCount, @WasVetoed,
         @WasVetoOverridden, @WasCommissionerOverridden, @RecordedAtUtc);
      """;

    const string insertVetoSql = """
      INSERT INTO reporting.veto_facts
        (id, pick_id, draft_id, draft_part_public_id, sequence, issued_by_kind,
         issued_by_id_value, issued_by_public_id, issued_by_name, is_overridden,
         overridden_by_kind, overridden_by_id_value, overridden_by_public_id,
         overridden_by_name, is_self_veto, recorded_at_utc)
      VALUES
        (@Id, @PickId, @DraftId, @DraftPartPublicId, @Sequence, @IssuedByKind,
         @IssuedByIdValue, @IssuedByPublicId, @IssuedByName, @IsOverridden,
         @OverriddenByKind, @OverriddenByIdValue, @OverriddenByPublicId,
         @OverriddenByName, @IsSelfVeto, @RecordedAtUtc);
      """;

    const string insertCreditSql = """
      INSERT INTO reporting.pick_credit_facts
        (id, pick_id, draft_id, draft_part_public_id, drafter_id_value,
         drafter_public_id, drafter_person_public_id, drafter_name, recorded_at_utc)
      VALUES
        (@Id, @PickId, @DraftId, @DraftPartPublicId, @DrafterIdValue,
         @DrafterPublicId, @DrafterPersonPublicId, @DrafterName, @RecordedAtUtc);
      """;

    await connection.ExecuteAsync(
      new CommandDefinition(
        insertPickSql,
        pickRows,
        transaction: transaction,
        cancellationToken: cancellationToken
      )
    );

    await connection.ExecuteAsync(
      new CommandDefinition(
        insertVetoSql,
        vetoRows,
        transaction: transaction,
        cancellationToken: cancellationToken
      )
    );

    await connection.ExecuteAsync(
      new CommandDefinition(
        insertCreditSql,
        creditRows,
        transaction: transaction,
        cancellationToken: cancellationToken
      )
    );

    await transaction.CommitAsync(cancellationToken);

    await _cacheService.RemoveAsync(
      ReportingCacheKeys.RecordBookCanonicalCacheKey,
      cancellationToken
    );
    await _cacheService.RemoveAsync(ReportingCacheKeys.RecordBookAllCacheKey, cancellationToken);

    return Result.Success();
  }
}
