namespace ScreenDrafts.Modules.Drafts.Features.Categories.Edit;

internal sealed class EditCategoryCommandHandler(ICategoryRepository categoryRepository)
  : ICommandHandler<EditCategoryCommand>
{
  private readonly ICategoryRepository _categoriesRepository = categoryRepository;

  public async Task<Result> Handle(EditCategoryCommand EditCategoryRequest, CancellationToken cancellationToken)
  {
    var category = await _categoriesRepository.GetByPublicIdAsync(EditCategoryRequest.PublicId, cancellationToken);

    if (category is null)
    {
      return Result.Failure(CategoryErrors.NotFound(EditCategoryRequest.PublicId));
    }

    if (EditCategoryRequest.Name is not null)
    {
      var renameResult = category.Rename(EditCategoryRequest.Name);
      if (renameResult.IsFailure)
      {
        return renameResult;
      }
    }

    if (EditCategoryRequest.Description is not null)
    {
      var updateDescriptionResult = category.ChangeDescription(EditCategoryRequest.Description);
      if (updateDescriptionResult.IsFailure)
      {
        return updateDescriptionResult;
      }
    }

    _categoriesRepository.Update(category);

    return Result.Success();
  }
}



