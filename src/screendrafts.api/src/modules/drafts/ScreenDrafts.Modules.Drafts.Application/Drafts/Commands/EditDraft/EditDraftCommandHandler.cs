namespace ScreenDrafts.Modules.Drafts.Application.Drafts.Commands.EditDraft;

internal sealed class EditDraftCommandHandler(
  IDraftsRepository draftsRepository)
  : ICommandHandler<EditDraftCommand>
{
  private readonly IDraftsRepository _draftsRepository = draftsRepository;
  public async Task<Result> Handle(EditDraftCommand request, CancellationToken cancellationToken)
  {
    var draftId = DraftId.Create(request.DraftId);

    var draft = await _draftsRepository.GetByIdAsync(draftId, cancellationToken);

    if (draft is null)
    {
      return Result.Failure<Guid>(DraftErrors.NotFound(request.DraftId));
    }

    var result = draft.EditDraft(
      Title.Create(request.Title),
      request.DraftType,
      request.Description);

    if (result.IsFailure)
    {
      return Result.Failure(result.Errors);
    }

    _draftsRepository.Update(draft);

    return Result.Success();
  }
}
