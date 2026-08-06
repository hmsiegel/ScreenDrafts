namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.SubDrafts.AssignSubDraftPosition;

internal sealed class AssignSubDraftPositionCommandHandler(
  IDraftPartRepository draftPartRepository,
  ParticipantResolver participantResolver
) : ICommandHandler<AssignSubDraftPositionCommand>
{
  private readonly IDraftPartRepository _draftPartRepository = draftPartRepository;
  private readonly ParticipantResolver _participantResolver = participantResolver;

  public async Task<Result> Handle(
    AssignSubDraftPositionCommand request,
    CancellationToken cancellationToken
  )
  {
    var draftPart = await _draftPartRepository.GetByPublicIdWithSubDraftsAsync(
      request.DraftPartId,
      cancellationToken
    );

    if (draftPart is null)
    {
      return Result.Failure(DraftPartErrors.NotFound(request.DraftPartId));
    }

    var subDraft = draftPart.SubDrafts.FirstOrDefault(x => x.PublicId == request.SubDraftId);

    if (subDraft is null)
    {
      return Result.Failure(SubDraftErrors.NotFound(request.SubDraftId));
    }

    var winnerResult = await _participantResolver.ResolveAsync(
      request.WinnerParticipantPublicId,
      request.WinnerParticipantKind,
      cancellationToken
    );

    if (winnerResult.IsFailure)
    {
      return Result.Failure(winnerResult.Errors);
    }

    var winner = winnerResult.Value;

    var validationResult = winner.Validate();

    if (validationResult.IsFailure)
    {
      return Result.Failure(validationResult.Errors);
    }

    var result = draftPart.AssignSubDraftPosition(subDraft.Id, winner, request.Choice);

    if (result.IsFailure)
    {
      return result;
    }

    _draftPartRepository.Update(draftPart);

    return Result.Success();
  }
}
