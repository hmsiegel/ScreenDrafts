namespace ScreenDrafts.Modules.Drafts.Features.Categories.Restore;

internal sealed class RestoreCategoryCommandHandler(ICategoryRepository categoriesRepository)
  : ICommandHandler<RestoreCategoryCommand>
{
  private readonly ICategoryRepository _categoriesRepository = categoriesRepository;

  public async Task<Result> Handle(RestoreCategoryCommand RestoreCategoryRequest, CancellationToken cancellationToken)
  {
    var category = await _categoriesRepository.GetByPublicIdAsync(RestoreCategoryRequest.PublicId, cancellationToken);

    if (category is null)
    {
      return Result.Failure(CategoryErrors.NotFound(RestoreCategoryRequest.PublicId));
    }

    var restoreResult = category.Restore();

    if (restoreResult.IsFailure)
    {
      return Result.Failure(restoreResult.Errors);
    }

    _categoriesRepository.Update(category);

    return Result.Success();
  }
}



