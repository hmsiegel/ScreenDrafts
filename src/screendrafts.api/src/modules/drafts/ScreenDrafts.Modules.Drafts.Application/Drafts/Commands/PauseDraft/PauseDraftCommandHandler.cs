namespace ScreenDrafts.Modules.Drafts.Application.Drafts.Commands.PauseDraft;

internal sealed class PauseDraftCommandHandler(
  IDraftsRepository draftsRepository)
  : ICommandHandler<PauseDraftCommand>
{
  private readonly IDraftsRepository _draftsRepository = draftsRepository;
  public async Task<Result> Handle(PauseDraftCommand request, CancellationToken cancellationToken)
  {
    var draftId = DraftId.Create(request.DraftId);
    var draft = await _draftsRepository.GetByIdAsync(draftId, cancellationToken);
    if (draft is null)
    {
      return Result.Failure<Guid>(DraftErrors.NotFound(request.DraftId));
    }

    var result = draft.PauseDraft();

    if (result.IsFailure)
    {
      return Result.Failure(result.Errors);
    }

    _draftsRepository.Update(draft);
    return Result.Success();
  }
}
