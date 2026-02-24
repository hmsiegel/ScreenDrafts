namespace ScreenDrafts.Modules.Drafts.Features.Drafts.SetCategories;

internal sealed class SetCategoriesDraftCommandHandler(
  IDraftRepository draftRepository,
  ICategoryRepository categoryRepository)
  : ICommandHandler<SetCategoriesDraftCommand>
{
  private readonly IDraftRepository _draftRepository = draftRepository;
  private readonly ICategoryRepository _categoryRepository = categoryRepository;

  public async Task<Result> Handle(SetCategoriesDraftCommand request, CancellationToken cancellationToken)
  {
    var draft = await _draftRepository.GetByPublicIdAsync(request.DraftId, cancellationToken);

    if (draft is null)
    {
      return Result.Failure(DraftErrors.NotFound(request.DraftId));
    }

    var categories = await _categoryRepository.GetByPublicIdsAsync(request.CategoryIds, cancellationToken);

    if (categories.Count != request.CategoryIds.Count)
    {
      return Result.Failure(DraftErrors.OneOrMoreCategoriesNotFound);
    }

    draft.SetCategories(categories);
    _draftRepository.Update(draft);
    return Result.Success();
  }
}
