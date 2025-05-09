namespace ScreenDrafts.Modules.Drafts.Application.Drafts.Commands.CompleteDraft;

internal sealed class CompleteDraftCommandHandler(
  IDraftsRepository draftsRepository)
  : ICommandHandler<CompleteDraftCommand>
{
  private readonly IDraftsRepository _draftsRepository = draftsRepository;

  public async Task<Result> Handle(CompleteDraftCommand request, CancellationToken cancellationToken)
  {
    var draft = await _draftsRepository.GetDraftWithDetailsAsync(DraftId.Create(request.DraftId), cancellationToken);

    if (draft is null)
    {
      return Result.Failure(DraftErrors.NotFound(request.DraftId));
    }

    draft.CompleteDraft();
    _draftsRepository.Update(draft);
    return Result.Success();
  }
}
