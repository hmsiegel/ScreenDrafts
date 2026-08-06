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

    SubDraftId? subDraftId = null;

    if (!string.IsNullOrWhiteSpace(request.SubDraftPublicId))
    {
      var subDraft = draftPart.SubDrafts.FirstOrDefault(x =>
        x.PublicId == request.SubDraftPublicId
      );

      if (subDraft is null)
      {
        return Result.Failure(SubDraftErrors.NotFound(request.SubDraftPublicId));
      }

      subDraftId = subDraft.Id;
    }

    var result = draftPart.UndoVeto(request.PlayOrder, subDraftId);

    if (result.IsFailure)
    {
      return result;
    }

    draftPartRepository.Update(draftPart);

    return Result.Success();
  }
}
