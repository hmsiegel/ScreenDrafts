namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.SubDrafts.AdvanceSubDraft;

internal sealed class AdvanceSubDraftCommandHandler(
  IDraftPartRepository draftPartRepository,
  IDbConnectionFactory dbConnectionFactory)
  : ICommandHandler<AdvanceSubDraftCommand>

{
  private readonly IDraftPartRepository _draftPartRepository = draftPartRepository;
  private readonly IDbConnectionFactory _dbConnectionFactory = dbConnectionFactory;

  public async Task<Result> Handle(AdvanceSubDraftCommand request, CancellationToken cancellationToken)
  {
    var draftPart = await _draftPartRepository.GetByPublicIdWithSubDraftsAsync(request.DraftPartPublicId, cancellationToken);

    if (draftPart is null)
      return Result.Failure(DraftPartErrors.NotFound(request.DraftPartPublicId));

    var subDraft = draftPart.SubDrafts.FirstOrDefault(x => x.PublicId == request.SubDraftPublicId);

    if (subDraft is null)
      return Result.Failure(SubDraftErrors.NotFound(request.SubDraftPublicId));

    await using var connection = await _dbConnectionFactory.OpenConnectionAsync(cancellationToken);

    const string sql =
      """
      SELECT
        v.sub_draft_id,
        v.is_overridden
      FROM drafts.vetoes v
      WHERE v.sub_draft_id = ANY(@SubDraftIds)
      """;

    var vetoRows = await connection.QueryAsync<(Guid SubDraftId, bool IsOverridden)>(
      new CommandDefinition(
        sql,
        new { SubDraftIds = draftPart.SubDrafts.Select(x => x.Id.Value).ToArray() },
        cancellationToken: cancellationToken));

    var vetoes = vetoRows
      .Select(v => (SubDraftId: SubDraftId.Create(v.SubDraftId), v.IsOverridden))
      .ToList();

    var advanceResult = draftPart.AdvanceSubDraft(subDraft.Id, vetoes);

    if (advanceResult.IsFailure)
      return advanceResult;

    var remainder = advanceResult.Value;

    var nexSubDraft = draftPart.SubDrafts
      .OrderBy(x => x.Index)
      .FirstOrDefault(x => x.Index > subDraft.Index);

    if (nexSubDraft is not null && remainder > 0)
    {
      foreach (var participant in draftPart.Participants)
      {
        var initResult = draftPart.InitializeParticipantVetoes(
          participant: participant,
          startingVetoes: 1,
          vetoesRollingIn: remainder,
          vetoOverridesRollingIn: 0);

        if (initResult.IsFailure)
          return initResult;
      }
    }

    _draftPartRepository.Update(draftPart);
    return Result.Success();
  }
}

