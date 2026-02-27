namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.AssignParticipantToDraftPosition;

internal sealed class AssignParticipantToDraftPositionCommandHandler : ICommandHandler<AssignParticipantToDraftPositionCommand>
{
  private readonly IDraftPartRepository _draftPartRepository;
  private readonly ParticipantResolver _participantResolver;
  public AssignParticipantToDraftPositionCommandHandler(IDraftPartRepository draftPartRepository, ParticipantResolver participantResolver)
  {
    _draftPartRepository = draftPartRepository;
    _participantResolver = participantResolver;
  }

  public async Task<Result> Handle(AssignParticipantToDraftPositionCommand request, CancellationToken cancellationToken)
  {
    var draftPart = await _draftPartRepository.GetByPublicIdAsync(request.DraftPartId, cancellationToken);

    if (draftPart is null)
    {
      return Result.Failure(DraftPartErrors.NotFound(request.DraftPartId));
    }

    if (draftPart.GameBoard is null)
    {
      return Result.Failure(DraftPartErrors.GameBoardNotFound);
    }

    var position = draftPart.GameBoard.DraftPositions.FirstOrDefault(p => p.PublicId == request.PositionPublicId);

    if (position is null)
    {
      return Result.Failure(DraftPositionErrors.NotFound(request.PositionPublicId));
    }

    var participantResult = await _participantResolver.ResolveAsync(
      request.ParticipantPublicId,
      request.ParticipantKind,
      cancellationToken);

    if (participantResult.IsFailure)
    {
      return Result.Failure(participantResult.Errors);
    }

    var participant = participantResult.Value;

    participant.Validate();

    var result = draftPart.AddParticipant(participant);

    if (result.IsFailure)
    {
      return result;
    }

    _draftPartRepository.Update(draftPart);

    return Result.Success();
  }
}
