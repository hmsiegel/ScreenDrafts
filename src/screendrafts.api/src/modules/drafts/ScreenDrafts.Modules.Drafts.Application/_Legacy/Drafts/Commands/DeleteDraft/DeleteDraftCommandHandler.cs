namespace ScreenDrafts.Modules.Drafts.Application._Legacy.Drafts.Commands.DeleteDraft;

internal sealed class DeleteDraftCommandHandler(IDraftRepository draftsRepository) : ICommandHandler<DeleteDraftCommand>
{
  private readonly IDraftRepository _draftsRepository = draftsRepository;

  public async Task<Result> Handle(DeleteDraftCommand command, CancellationToken cancellationToken)
  {
    var draftId = DraftId.Create(command.DraftId);
    var draft = await _draftsRepository.GetByIdAsync(draftId, cancellationToken);
    if (draft is null)
    {
      return Result.Failure(DraftErrors.NotFound(command.DraftId));
    }

    _draftsRepository.Delete(draft);

    return Result.Success();
  }
}
