namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.DraftPositions.ClearDraftPositionAssignment;

internal sealed class ClearDraftPositionAssignmentCommandHandler(IDraftPartRepository draftPartRepository)
  : ICommandHandler<ClearDraftPositionAssignmentCommand>
{
  private readonly IDraftPartRepository _draftPartRepository = draftPartRepository;

  public async Task<Result> Handle(ClearDraftPositionAssignmentCommand request, CancellationToken cancellationToken)
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

    var result = draftPart.ClearPositionAssignment(position);

    if (result.IsFailure)
    {
      return Result.Failure(result.Errors);
    }

    _draftPartRepository.Update(draftPart);

    return Result.Success();
  }
}
