namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.DraftPositions.SetPartPositionRange;

internal sealed class SetPartPositionRangeCommandHandler(IDraftPartRepository draftPartRepository)
  : ICommandHandler<SetPartPositionRangeCommand>
{
  private readonly IDraftPartRepository _draftPartRepository = draftPartRepository;

  public async Task<Result> Handle(
    SetPartPositionRangeCommand request,
    CancellationToken cancellationToken
  )
  {
    var draftPart = await _draftPartRepository.GetByPublicIdAsync(
      request.DraftPartId,
      cancellationToken
    );

    if (draftPart is null)
    {
      return Result.Failure(DraftPartErrors.NotFound(request.DraftPartId));
    }

    var result = draftPart.SetPartPositions(request.MinimumPosition, request.MaximumPosition);

    if (result.IsFailure)
    {
      return result;
    }

    _draftPartRepository.Update(draftPart);

    return Result.Success();
  }
}
