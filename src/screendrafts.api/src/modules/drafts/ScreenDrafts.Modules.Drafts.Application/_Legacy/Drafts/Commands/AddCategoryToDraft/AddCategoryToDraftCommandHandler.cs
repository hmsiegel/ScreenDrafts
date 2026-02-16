using ScreenDrafts.Modules.Drafts.Domain.Categories;

namespace ScreenDrafts.Modules.Drafts.Application._Legacy.Drafts.Commands.AddCategoryToDraft;

internal sealed class AddCategoryToDraftCommandHandler(
    IDraftRepository draftsRepository,
    ICategoryRepository categoriesRepository)
    : ICommandHandler<AddCategoryToDraftCommand, Guid>
{
  private readonly IDraftRepository _draftsRepository = draftsRepository;
  private readonly ICategoryRepository _categoriesRepository = categoriesRepository;

  public async Task<Result<Guid>> Handle(AddCategoryToDraftCommand request, CancellationToken cancellationToken)
  {
    var draftId = DraftId.Create(request.DraftId);
    var draft = await _draftsRepository.GetByIdAsync(draftId, cancellationToken);

    if (draft is null)
    {
      return Result.Failure<Guid>(DraftErrors.NotFound(request.DraftId));
    }

    var categoryId = CategoryId.Create(request.CategoryId);
    var category = await _categoriesRepository.GetByIdAsync(categoryId, cancellationToken);

    if (category is null)
    {
      return Result.Failure<Guid>(CategoryErrors.NotFound(request.CategoryId));
    }

    draft.AddCategory(category);
    _draftsRepository.Update(draft);
    return Result.Success(category.Id.Value);
  }
}
