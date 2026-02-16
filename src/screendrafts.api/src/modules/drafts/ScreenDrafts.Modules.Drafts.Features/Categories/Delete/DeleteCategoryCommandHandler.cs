namespace ScreenDrafts.Modules.Drafts.Features.Categories.Delete;

internal sealed class DeleteCategoryCommandHandler(ICategoryRepository categoriesRepository)
  : ICommandHandler<DeleteCategoryCommand>
{
  private readonly ICategoryRepository _categoriesRepository = categoriesRepository;

  public async Task<Result> Handle(DeleteCategoryCommand DeleteCategoryRequest, CancellationToken cancellationToken)
  {
    var category = await _categoriesRepository.GetByPublicIdAsync(DeleteCategoryRequest.PublicId, cancellationToken);

    if (category is null)
    {
      return Result.Failure(CategoryErrors.NotFound(DeleteCategoryRequest.PublicId));
    }

    if (category.IsDeleted)
    {
      return Result.Success();
    }

    var deleteResult = category.SoftDelete();

    if (!deleteResult.IsFailure)
    {
      return Result.Failure(CategoryErrors.DeletionFailed(DeleteCategoryRequest.PublicId));
    }

    return Result.Success();
  }
}



