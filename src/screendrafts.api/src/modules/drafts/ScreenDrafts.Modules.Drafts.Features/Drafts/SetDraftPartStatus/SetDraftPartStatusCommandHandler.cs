namespace ScreenDrafts.Modules.Drafts.Features.Drafts.SetDraftPartStatus;

internal sealed class SetDraftPartStatusCommandHandler(
  IDraftRepository draftsRepository,
  IDateTimeProvider dateTimeProvider,
  IDbConnectionFactory dbConnectionFactory
) : ICommandHandler<SetDraftPartStatusCommand, SetDraftPartStatusResponse>
{
  private readonly IDraftRepository _draftsRepository = draftsRepository;
  private readonly IDateTimeProvider _dateTimeProvider = dateTimeProvider;
  private readonly IDbConnectionFactory _dbConnectionFactory = dbConnectionFactory;

  public async Task<Result<SetDraftPartStatusResponse>> Handle(
    SetDraftPartStatusCommand request,
    CancellationToken cancellationToken
  )
  {
    var utcNow = _dateTimeProvider.UtcNow;

    var draft = await _draftsRepository.GetDraftByPublicIdWithPartsAsync(
      request.DraftPublicId,
      cancellationToken
    );

    if (draft is null)
    {
      return Result<SetDraftPartStatusResponse>.ValidationFailure(
        DraftErrors.NotFound(request.DraftPublicId)
      );
    }

    var part = draft.Parts.FirstOrDefault(p => p.PartIndex == request.PartIndex);

    if (part is null)
    {
      return Result.Failure<SetDraftPartStatusResponse>(
        DraftErrors.DraftPartNotFoundByIndex(request.DraftPublicId, request.PartIndex)
      );
    }

    if (request.Action == DraftPartStatusAction.Start)
    {
      var rolloverResult = await ApplyRolloversAsync(draft, part, cancellationToken);
      if (rolloverResult.IsFailure)
      {
        return Result.Failure<SetDraftPartStatusResponse>(rolloverResult.Errors[0]);
      }
    }

    var result = request.Action switch
    {
      DraftPartStatusAction.Start => draft.StartPart(part.Id, utcNow),
      DraftPartStatusAction.Complete => draft.CompletePart(part.Id, utcNow),
      _ => Result.Failure<SetDraftPartStatusResponse>(DraftErrors.InvalidDraftPartStatusAction),
    };

    if (result.IsFailure)
    {
      return Result.Failure<SetDraftPartStatusResponse>(result.Errors[0]);
    }

    _draftsRepository.Update(draft);

    var draftLifecycle = draft.GetLifecycleView(utcNow);
    var draftPartLifecycle = part.GetLifecycleView(utcNow);

    return Result.Success(
      new SetDraftPartStatusResponse
      {
        DraftPublicId = draft.PublicId,
        PartIndex = part.PartIndex,
        DraftPartId = part.Id.Value,
        DraftStatus = draft.DraftStatus.Name,
        DraftLifecylce = draftLifecycle.ToString(),
        DraftPartStatus = part.Status.ToString(),
        DraftPartLifecycle = draftPartLifecycle.ToString(),
      }
    );
  }

  private async Task<Result> ApplyRolloversAsync(
    Draft draft,
    DraftPart part,
    CancellationToken cancellationToken
  )
  {
    // Draft.FungibleTokenName is the only signal for "this draft uses a fungible token" —
    // NOT SeriesKind. Two drafts in the same series (e.g. two different years' Legends
    // Mega) can differ on this, as the most recent Legends Mega proved.
    var usesFungibleToken = draft.FungibleTokenName is not null;

    int startingVetoes;
    if (usesFungibleToken)
    {
      startingVetoes = 0;
    }
    else
    {
      startingVetoes = (part.PartIndex == 1 || draft.GrantsStartingVetoPerPart) ? 1 : 0;
    }

    var fungibleTokensStarting =
      usesFungibleToken && (part.PartIndex == 1 || draft.GrantsStartingVetoPerPart) ? 1 : 0;

    var drafterIds = part.Participants.Where(p => p.IsDrafter).Select(p => p.Value).ToArray();

    if (drafterIds.Length == 0)
    {
      return Result.Success();
    }

    // Fungible-token rollover is intra-draft only — a single-draft allocation that should
    // never carry across separate drafts, even within the same series/continuity scope
    // (last year's Legends Mega token must never leak into this year's). Computed directly
    // off the Draft aggregate's own parts, already in memory here, independent of the
    // ContinuityScope-driven query below, which governs the normal veto/override pools only.
    // A single-part draft (e.g. the Reiner Run) simply has no prior part, so this is
    // naturally 0 for it without any special-casing.
    var priorPart = draft
      .Parts.Where(p => p.Id != part.Id && p.Status == DraftPartStatus.Completed)
      .OrderByDescending(p => p.PartIndex)
      .FirstOrDefault();

    var continuityScope = draft.Series.ContinuityScope;

    if (continuityScope == ContinuityScope.None || continuityScope == ContinuityScope.SpeedDrafts)
    {
      return ApplyZeroRollovers(
        part,
        startingVetoes,
        fungibleTokensStarting,
        priorPart,
        drafterIds
      );
    }

    string scopeFilter;
    object parameters;

    if (continuityScope == ContinuityScope.Series)
    {
      scopeFilter = "AND d.SeriesId = @SeriesId";
      parameters = new
      {
        DrafterIds = drafterIds,
        CurrentPartIds = part.Id.Value,
        SeriesId = draft.SeriesId.Value,
      };
    }
    else // Global
    {
      scopeFilter = string.Empty;

      parameters = new { DrafterIds = drafterIds, CurrentPartIds = part.Id.Value };
    }

    var sql = $"""
      SELECT DISTINCT ON (dpp.participant_id_value)
        dpp.participant_id_value                                          AS ParticipantId,
        CASE WHEN (dpp.starting_vetoes
                   + dpp.vetoes_rolling_in
                   + dpp.awarded_vetoes
                   - dpp.vetoes_used) >= 1 THEN 1 ELSE 0 END              AS VetoesRollingOut,
        CASE WHEN (dpp.veto_overrides_rolling_in
                   + dpp.awarded_veto_overrides
                   - dpp.veto_overrides_used) >= 1 THEN 1 ELSE 0 END      AS VetoOverridesRollingOut
      FROM drafts.draft_part_participants dpp
      INNER JOIN drafts.draft_parts dp ON dp.id = dpp.draft_part_id
      INNER JOIN drafts.drafts d ON d.id = dp.draft_id
      WHERE dpp.participant_id_value = ANY(@DrafterIds)
        AND dpp.participant_kind_value = 0
        AND dp.status = 3
        AND dp.id != @CurrentPartIds
        {scopeFilter}
      ORDER BY dpp.participant_id_value, dp.created_at_utc DESC
      """;

    await using var connection = await _dbConnectionFactory.OpenConnectionAsync(cancellationToken);

    // S2077: sql interpolates scopeFilter, which is one of exactly two hardcoded clauses chosen by the ContinuityScope branch above; all values are bound via Dapper parameters.
#pragma warning disable S2077
    var rows = await connection.QueryAsync<RolloverRow>(
      new CommandDefinition(sql, parameters, cancellationToken: cancellationToken)
    );
#pragma warning restore S2077

    var rolloverByDrafter = rows.ToDictionary(r => r.ParticipantId);

    foreach (var participant in part.Participants.Where(p => p.IsDrafter))
    {
      rolloverByDrafter.TryGetValue(participant.Value, out var prior);

      var fungibleTokensRollingIn =
        priorPart?.FindParticipant(participant)?.FungibleTokensRollingOut ?? 0;

      var initResult = part.InitializeParticipantVetoes(
        participant,
        startingVetoes,
        prior?.VetoesRollingOut ?? 0,
        prior?.VetoOverridesRollingOut ?? 0,
        fungibleTokensStarting,
        fungibleTokensRollingIn
      );

      if (initResult.IsFailure)
      {
        return initResult;
      }
    }

    return Result.Success();
  }

  private static Result ApplyZeroRollovers(
    DraftPart part,
    int startingVetoes,
    int fungibleTokensStarting,
    DraftPart? priorPart,
    Guid[] drafterIds
  )
  {
    foreach (
      var participant in part.Participants.Where(p => p.IsDrafter && drafterIds.Contains(p.Value))
    )
    {
      // Even when ContinuityScope blocks normal veto/override rollover (None/SpeedDrafts),
      // fungible-token rollover still applies if this draft has a completed prior part —
      // the two are independent mechanisms. See remarks above.
      var fungibleTokensRollingIn =
        priorPart?.FindParticipant(participant)?.FungibleTokensRollingOut ?? 0;

      var initResult = part.InitializeParticipantVetoes(
        participant,
        startingVetoes,
        0,
        0,
        fungibleTokensStarting,
        fungibleTokensRollingIn
      );
      if (initResult.IsFailure)
      {
        return initResult;
      }
    }

    return Result.Success();
  }

  private sealed record RolloverRow(
    Guid ParticipantId,
    int VetoesRollingOut,
    int VetoOverridesRollingOut
  );
}
