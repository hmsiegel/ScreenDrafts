namespace ScreenDrafts.Modules.Drafts.Application.Drafts.Commands.CompleteDraft;

internal sealed class CompleteDraftCommandHandler(
  IDraftsRepository draftsRepository,
  IUnitOfWork unitOfWork)
  : ICommandHandler<CompleteDraftCommand>
{
  private readonly IDraftsRepository _draftsRepository = draftsRepository;
  private readonly IUnitOfWork _unitOfWork = unitOfWork;

  public async Task<Result> Handle(CompleteDraftCommand request, CancellationToken cancellationToken)
  {
    var draft = await _draftsRepository.GetByIdAsync(DraftId.Create(request.DraftId), cancellationToken);

    if (draft is null)
    {
      return Result.Failure(DraftErrors.NotFound(request.DraftId));
    }

    draft.CompleteDraft();
    _draftsRepository.Update(draft);
    await _unitOfWork.SaveChangesAsync(cancellationToken);
    return Result.Success();
  }
}
