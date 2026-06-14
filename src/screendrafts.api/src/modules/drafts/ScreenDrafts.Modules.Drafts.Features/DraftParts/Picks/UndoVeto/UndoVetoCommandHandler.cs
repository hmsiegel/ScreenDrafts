namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.Picks.UndoVeto;

// ── Command Handler ───────────────────────────────────────────────────────────

internal sealed class UndoVetoCommandHandler(IDraftPartRepository draftPartRepository)
  : ICommandHandler<UndoVetoCommand>
{
  public async Task<Result> Handle(UndoVetoCommand request, CancellationToken cancellationToken)
  {
    var draftPart = await draftPartRepository.GetByPublicIdAsync(
      request.DraftPartId,
      cancellationToken
    );

    if (draftPart is null)
    {
      return Result.Failure(DraftPartErrors.NotFound(request.DraftPartId));
    }

    var result = draftPart.UndoVeto(request.PlayOrder);

    if (result.IsFailure)
    {
      return result;
    }

    draftPartRepository.Update(draftPart);

    return Result.Success();
  }
}
