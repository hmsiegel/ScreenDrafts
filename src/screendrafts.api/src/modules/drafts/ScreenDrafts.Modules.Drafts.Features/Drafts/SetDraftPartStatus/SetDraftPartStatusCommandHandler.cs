using ScreenDrafts.Modules.Drafts.Domain.DraftParts;

namespace ScreenDrafts.Modules.Drafts.Features.Drafts.SetDraftPartStatus;

internal sealed class SetDraftPartStatusCommandHandler(
  IDraftRepository draftsRepository,
  IDateTimeProvider dateTimeProvider,
  IDbConnectionFactory dbConnectionFactory)
  : ICommandHandler<SetDraftPartStatusCommand, Response>
{
  private readonly IDraftRepository _draftsRepository = draftsRepository;
  private readonly IDateTimeProvider _dateTimeProvider = dateTimeProvider;
  private readonly IDbConnectionFactory _dbConnectionFactory = dbConnectionFactory;

  public async Task<Result<Response>> Handle(SetDraftPartStatusCommand request, CancellationToken cancellationToken)
  {
    var utcNow = _dateTimeProvider.UtcNow;

    var draft = await _draftsRepository.GetDraftByPublicIdWithPartsAsync(request.DraftPublicId, cancellationToken);

    if (draft is null)
    {
      return Result<Response>.ValidationFailure(DraftErrors.NotFound(request.DraftPublicId));
    }

    var part = draft.Parts.FirstOrDefault(p => p.PartIndex == request.PartIndex);

    if (part is null)
    {
      return Result.Failure<Response>(DraftErrors.DraftPartNotFoundByIndex(request.DraftPublicId, request.PartIndex));
    }

    if (request.Action == DraftPartStatusAction.Start)
    {
      var rolloverResult = await ApplyRolloversAsync(draft, part, cancellationToken);
      if (rolloverResult.IsFailure)
      {
        return Result.Failure<Response>(rolloverResult.Errors[0]);
      }
    }

    var result = request.Action switch
    {
      DraftPartStatusAction.Start => draft.StartPart(part.Id, utcNow),
      DraftPartStatusAction.Complete => draft.CompletePart(part.Id, utcNow),
      _ => Result.Failure<Response>(DraftErrors.InvalidDraftPartStatusAction)
    };

    if (result.IsFailure)
    {
      return Result.Failure<Response>(result.Errors[0]);
    }

    _draftsRepository.Update(draft);

    var draftLifecycle = draft.GetLifecycleView(utcNow);
    var draftPartLifecycle = part.GetLifecycleView(utcNow);

    return Result.Success(new Response
    {
      DraftPublicId = draft.PublicId,
      PartIndex = part.PartIndex,
      DraftPartId = part.Id.Value,
      DraftStatus = draft.DraftStatus.Name,
      DraftLifecylce = draftLifecycle.ToString(),
      DraftPartStatus = part.Status.ToString(),
      DraftPartLifecycle = draftPartLifecycle.ToString()
    });
  }

  private async Task<Result> ApplyRolloversAsync(
    Draft draft,
    DraftPart part,
    CancellationToken cancellationToken)
  {
    var startingVetoes = (part.PartIndex == 1 || draft.GrantsStartingVetoPerPart) ? 1 : 0;

    var drafterIds = part.Participants
      .Where(p => p.IsDrafter)
      .Select(p => p.Value)
      .ToArray();

    if (drafterIds.Length == 0)
    {
      return Result.Success();
    }

    var continuityScope = draft.Series.ContinuityScope;

    if (continuityScope == ContinuityScope.None || continuityScope == ContinuityScope.SpeedDrafts)
    {
      return ApplyZeroRollovers(part, startingVetoes, drafterIds);
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
        SeriesId = draft.SeriesId.Value
      };
    }
    else // Global
    {
      var channel = draft.ChannelReleases.Any(cr => cr.ReleaseChannel == ReleaseChannel.MainFeed)
        ? ReleaseChannel.MainFeed.Value
        : ReleaseChannel.Patreon.Value;

      scopeFilter =
        """
        AND EXISTS (
          SELECT 1
          FROM drafts.draft_channel_releases dcr
          WHERE dcr.draft_id = d.id
            AND dcr.releas_channel = @Channel
        )
        """;

      parameters = new
      {
        DrafterIds = drafterIds,
        CurrentPartIds = part.Id.Value,
        Channel = channel
      };
    }

    var sql =
      $"""
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

    var rows = await connection.QueryAsync<RolloverRow>(
      new CommandDefinition(
        sql,
        parameters,
        cancellationToken: cancellationToken));

    var rolloverByDrafter = rows.ToDictionary(r => r.ParticipantId);

    foreach (var participant in part.Participants
                 .Where(p => p.IsDrafter))
    {
      rolloverByDrafter.TryGetValue(participant.Value, out var prior);

      var initResult = part.InitializeParticipantVetoes(
        participant,
        startingVetoes,
        prior?.VetoesRollingOut ?? 0,
        prior?.VetoOverridesRollingOut ?? 0);

      if (initResult.IsFailure)
      {
        return initResult;
      }
    }

    return Result.Success();
  }

  private static Result ApplyZeroRollovers(DraftPart part, int startingVetoes, Guid[] drafterIds)
  {
    foreach (var participant in part.Participants
                .Where(p => p.IsDrafter && drafterIds.Contains(p.Value)))
    {
      var initResult = part.InitializeParticipantVetoes(participant, startingVetoes, 0, 0);
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
    int VetoOverridesRollingOut);
}



