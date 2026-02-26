namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.ApplyCommissionerOverride;

internal sealed class ApplyCommissionerOverrideCommandHandler(
  IDraftPartRepository draftPartRepository,
  IPickRepository pickRepository)
  : ICommandHandler<ApplyCommissionerOverrideCommand>
{
  private readonly IDraftPartRepository _draftPartRepository = draftPartRepository;
  private readonly IPickRepository _pickRepository = pickRepository;

  public async Task<Result> Handle(ApplyCommissionerOverrideCommand request, CancellationToken cancellationToken)
  {
    var draftPart = await _draftPartRepository.GetByPublicIdAsync(request.DraftPartId, cancellationToken);

    if (draftPart is null)
    {
      return Result.Failure(DraftPartErrors.NotFound(request.DraftPartId));
    }

    var pick = await _pickRepository.GetByDraftPartIdAndPlayOrderAsync(draftPart.Id, request.PlayOrder, cancellationToken);

    if (pick is null)
    {
      return Result.Failure(DraftPartErrors.PickNotFound(request.PlayOrder));
    }

    var applyResult = draftPart.ApplyCommissionerOverride(pick);

    if (applyResult.IsFailure)
    {
      return applyResult;
    }

    _draftPartRepository.Update(draftPart);

    return Result.Success();
  }
}


