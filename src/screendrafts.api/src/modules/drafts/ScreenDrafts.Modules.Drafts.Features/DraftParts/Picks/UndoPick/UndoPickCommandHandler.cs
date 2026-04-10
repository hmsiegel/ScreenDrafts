namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.Picks.UndoPick;

internal sealed class UndoPickCommandHandler(
  IDraftPartRepository draftPartRepository,
  IPickRepository pickRepository)
  : ICommandHandler<UndoPickCommand>
{
  private readonly IDraftPartRepository _draftPartRepository = draftPartRepository;
  private readonly IPickRepository _pickRepository = pickRepository;

  public async Task<Result> Handle(UndoPickCommand request, CancellationToken cancellationToken)
  {
    var draftPart = await _draftPartRepository.GetByPublicIdAsync(request.DraftPartPublicId, cancellationToken);

    if (draftPart is null)
    {
      return Result.Failure(DraftPartErrors.NotFound(request.DraftPartPublicId));
    }

    Pick? pick;

    if (!string.IsNullOrWhiteSpace(request.SubDraftPublicId))
    {
      var subDraft = draftPart.SubDrafts
        .FirstOrDefault(s => s.PublicId == request.SubDraftPublicId);

      if (subDraft is null)
      {
        return Result.Failure(SubDraftErrors.NotFound(request.SubDraftPublicId));
      }

      pick = await _pickRepository.GetByDraftPartIdAndPlayOrderAsync(
        id: draftPart.Id,
        playOrder: request.PlayOrder,
        subDraftId: subDraft.Id,
        cancellationToken: cancellationToken);
    }
    else
    {
       pick = await _pickRepository.GetByDraftPartIdAndPlayOrderAsync(
        id: draftPart.Id,
        playOrder: request.PlayOrder,
        cancellationToken: cancellationToken);
    }

    if (pick is null)
    {
      return Result.Failure(DraftPartErrors.PickNotFound(request.PlayOrder));
    }

    if (pick.IsVetoed)
    {
      return Result.Failure(PickErrors.CannotUndoAVetoedPick(request.PlayOrder));
    }

    if (pick.IsCommissionerOverridden)
    {
      return Result.Failure(PickErrors.CannotUndoACommissionerOverriddenPick(request.PlayOrder));
    }

    var result = draftPart.UndoPick(request.PlayOrder);

    if (result.IsFailure)
    {
      return Result.Failure(result.Errors);
    }

    _pickRepository.Delete(pick);

    return Result.Success();
  }
}
