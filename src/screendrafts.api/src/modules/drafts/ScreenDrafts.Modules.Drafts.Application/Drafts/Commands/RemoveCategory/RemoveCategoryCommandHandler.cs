namespace ScreenDrafts.Modules.Drafts.Application.Drafts.Commands.RemoveCategory;

internal sealed class RemoveCategoryCommandHandler(
    ICategoriesRepository categoriesRepository)
    : ICommandHandler<RemoveCategoryCommand>
{
  private readonly ICategoriesRepository _categoriesRepository = categoriesRepository;
  public async Task<Result> Handle(RemoveCategoryCommand request, CancellationToken cancellationToken)
  {
    var categoryId = CategoryId.Create(request.CategoryId);
    var category = await _categoriesRepository.GetByIdAsync(categoryId, cancellationToken);
    if (category is null)
    {
      return Result.Failure(CategoryErrors.NotFound(request.CategoryId));
    }
    var isCategoryInUse = await _categoriesRepository.IsCategoryInUseAsync(categoryId, cancellationToken);

    if (isCategoryInUse)
    {
      return Result.Failure(CategoryErrors.CategoryStillInUse(request.CategoryId));
    }

    _categoriesRepository.RemoveCategory(category);

    return Result.Success();
  }
}
