namespace ScreenDrafts.Modules.Drafts.Features.Drafts.RemoveCategoryFromDraft;

internal sealed class RemoveCategoryFromDraftCommandHandler(
  IDraftRepository draftRepository,
  ICategoryRepository categoryRepository)
  : ICommandHandler<RemoveCategoryFromDraftCommand>
{
  private readonly IDraftRepository _draftRepository = draftRepository;
  private readonly ICategoryRepository _categoryRepository = categoryRepository;

  public async Task<Result> Handle(RemoveCategoryFromDraftCommand request, CancellationToken cancellationToken)
  {
    var draft = await _draftRepository.GetByPublicIdAsync(request.DraftId, cancellationToken);

    if (draft is null)
    {
      return Result.Failure(DraftErrors.NotFound(request.DraftId));
    }

    var category = await _categoryRepository.GetByPublicIdAsync(request.CategoryId, cancellationToken);
    if (category is null)
    {
      return Result.Failure(CategoryErrors.NotFound(request.CategoryId));
    }

    var result = draft.RemoveCategory(category);

    if (result.IsFailure)
    {
      return Result.Failure(result.Errors);
    }

    _draftRepository.Update(draft);

    return Result.Success();
  }
}
