namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.Picks.RevealPick;

internal sealed class RevealPickCommandHandler(
  IDraftPartRepository draftPartRepository,
  IPickRepository pickRepository)
  : ICommandHandler<RevealPickCommand>
{
  private readonly IDraftPartRepository _draftPartRepository = draftPartRepository;
  private readonly IPickRepository _pickRepository = pickRepository;

  public async Task<Result> Handle(RevealPickCommand request, CancellationToken cancellationToken)
  {
    var draftPart = await _draftPartRepository.GetByPublicIdWithHostsAsync(request.DraftPartId, cancellationToken);

    if (draftPart is null)
    {
      return Result.Failure(DraftPartErrors.NotFound(request.DraftPartId));
    }

    if (!draftPart.IsPrimaryHost(request.ActorPublicId))
    {
      return Result.Failure(DraftPartErrors.OnlyPrimaryHostCanRevealPicks);
    }

    var pick = await _pickRepository.GetByDraftPartIdAndPlayOrderAsync(
      id: draftPart.Id,
      playOrder: request.PlayOrder,
      cancellationToken: cancellationToken);

    if (pick is null)
    {
      return Result.Failure(DraftPartErrors.PickNotFound(request.PlayOrder));
    }

    var result = draftPart.RevealPick(
      playOrder: request.PlayOrder,
      actedByPublicId: request.ActorPublicId);

    if (result.IsFailure)
    {
      return result;
    }

    _draftPartRepository.Update(draftPart);

    return Result.Success();
  }
}
