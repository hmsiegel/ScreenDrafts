namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.SubDrafts.AdvanceSubDraft;

internal sealed class AdvanceSubDraftCommandHandler(
  IDraftPartRepository draftPartRepository,
  IDbConnectionFactory dbConnectionFactory
) : ICommandHandler<AdvanceSubDraftCommand>
{
  private readonly IDraftPartRepository _draftPartRepository = draftPartRepository;
  private readonly IDbConnectionFactory _dbConnectionFactory = dbConnectionFactory;

  public async Task<Result> Handle(
    AdvanceSubDraftCommand request,
    CancellationToken cancellationToken
  )
  {
    var draftPart = await _draftPartRepository.GetByPublicIdWithSubDraftsAsync(
      request.DraftPartPublicId,
      cancellationToken
    );

    if (draftPart is null)
    {
      return Result.Failure(DraftPartErrors.NotFound(request.DraftPartPublicId));
    }

    var subDraft = draftPart.SubDrafts.FirstOrDefault(x => x.PublicId == request.SubDraftPublicId);

    if (subDraft is null)
    {
      return Result.Failure(SubDraftErrors.NotFound(request.SubDraftPublicId));
    }

    await using var connection = await _dbConnectionFactory.OpenConnectionAsync(cancellationToken);

    const string sql = """
      SELECT
        v.sub_draft_id,
        dpp.participant_id_value,
        dpp.participant_kind_value
      FROM drafts.vetoes v
      JOIN drafts.draft_part_participants dpp ON v.issued_by_participant_id = dpp.id
      WHERE v.sub_draft_id = ANY(@SubDraftIds)
      """;

    var vetoRows = await connection.QueryAsync<(
      Guid SubDraftId,
      Guid ParticipantIdValue,
      int ParticipantKindValue
    )>(
      new CommandDefinition(
        sql,
        new { SubDraftIds = draftPart.SubDrafts.Select(x => x.Id.Value).ToArray() },
        cancellationToken: cancellationToken
      )
    );

    var vetoes = vetoRows
      .Select(v =>
        (
          SubDraftId: SubDraftId.Create(v.SubDraftId),
          IssuedBy: new Participant(
            v.ParticipantIdValue,
            ParticipantKind.FromValue(v.ParticipantKindValue)
          )
        )
      )
      .ToList();

    var advanceResult = draftPart.AdvanceSubDraft(subDraft.Id, vetoes);

    if (advanceResult.IsFailure)
      return advanceResult;

    var remainders = advanceResult.Value;

    var nexSubDraft = draftPart
      .SubDrafts.OrderBy(x => x.Index)
      .FirstOrDefault(x => x.Index > subDraft.Index);

    if (nexSubDraft is not null)
    {
      foreach (var participant in draftPart.Participants)
      {
        var remainder = remainders.GetValueOrDefault(participant, 0);
        var initResult = draftPart.InitializeParticipantVetoes(
          participant: participant,
          startingVetoes: 1,
          vetoesRollingIn: remainder,
          vetoOverridesRollingIn: 0
        );

        if (initResult.IsFailure)
          return initResult;
      }
    }

    _draftPartRepository.Update(draftPart);
    return Result.Success();
  }
}
