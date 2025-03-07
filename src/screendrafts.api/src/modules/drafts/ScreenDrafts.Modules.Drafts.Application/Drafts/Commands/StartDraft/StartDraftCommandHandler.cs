namespace ScreenDrafts.Modules.Drafts.Application.Drafts.Commands.StartDraft;

internal sealed class StartDraftCommandHandler(
  IDraftsRepository draftsRepository,
  IUnitOfWork unitOfWork)
  : ICommandHandler<StartDraftCommand>
{
  private readonly IDraftsRepository _draftsRepository = draftsRepository;
  private readonly IUnitOfWork _unitOfWork = unitOfWork;
  public async Task<Result> Handle(StartDraftCommand request, CancellationToken cancellationToken)
  {
    var draftId = DraftId.Create(request.DraftId);
    var draft = await _draftsRepository.GetDraftWithDetailsAsync(draftId, cancellationToken);
    if (draft is null)
    {
      return Result.Failure<Guid>(DraftErrors.NotFound(request.DraftId));
    }

    var result = draft.StartDraft();

    if (result.IsFailure)
    {
      return Result.Failure(result.Errors);
    }

    _draftsRepository.Update(draft);
    await _unitOfWork.SaveChangesAsync(cancellationToken);
    return Result.Success();
  }
}
