namespace ScreenDrafts.Modules.Drafts.Application.Drafts.Commands.ContinueDraftPart;

internal sealed class ContinueDraftPartCommandHandler(
    IDraftsRepository draftsRepository)
    : ICommandHandler<ContinueDraftPartCommand>
{
  private readonly IDraftsRepository _draftsRepository = draftsRepository;

  public async Task<Result> Handle(ContinueDraftPartCommand request, CancellationToken cancellationToken)
  {
    var draftPart = await _draftsRepository.GetDraftPartByIdAsync(
      new DraftPartId(request.DraftPartId), cancellationToken);

    if (draftPart is null)
    {
      return Result.Failure(DraftErrors.DraftPartNotFound(request.DraftPartId));
    }

    var draft = await _draftsRepository.GetDraftByDraftPartId(draftPart.Id, cancellationToken);

    if (draft is null)
    {
      return Result.Failure(DraftErrors.NotFound(draft!.Id.Value));
    }

    draftPart.ContinueDraft();
    draft.DeriveDraftStatus();
    _draftsRepository.Update(draft);
    return Result.Success();
  }
}
