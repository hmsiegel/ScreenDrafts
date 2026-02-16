using ScreenDrafts.Modules.Drafts.Domain.Categories;

namespace ScreenDrafts.Modules.Drafts.Application._Legacy.Drafts.Commands.RemoveCategoryFromDraft;

internal sealed class RemoveCategoryFromDraftCommandHandler(IDraftRepository draftsRepository, ICategoryRepository categoriesRepository)
        : ICommandHandler<RemoveCategoryFromDraftCommand>
{
  private readonly IDraftRepository _draftsRepository = draftsRepository;
  private readonly ICategoryRepository _categoriesRepository = categoriesRepository;

  public async Task<Result> Handle(RemoveCategoryFromDraftCommand request, CancellationToken cancellationToken)
  {
    var draftId = DraftId.Create(request.DraftId);
    var categoryId = CategoryId.Create(request.CategoryId);

    var draft = await _draftsRepository.GetByIdAsync(draftId, cancellationToken);

    if (draft is null)
    {
      return Result.Failure(DraftErrors.NotFound(request.DraftId));
    }

    var category = await _categoriesRepository.GetByIdAsync(categoryId, cancellationToken);

    if (category is null)
    {
      return Result.Failure(CategoryErrors.NotFound(request.CategoryId));
    }

    var result = draft.RemoveCategory(category);

    if (result.IsFailure)
    {
      return Result.Failure(result.Errors);
    }

    _draftsRepository.Update(draft);

    return Result.Success();
  }
}
