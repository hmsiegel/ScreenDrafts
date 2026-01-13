namespace ScreenDrafts.Modules.Drafts.Features.Categories.Edit;

internal sealed class CommandHandler(ICategoriesRepository categoryRepository)
  : Common.Features.Abstractions.Messaging.ICommandHandler<Command>
{
  private readonly ICategoriesRepository _categoriesRepository = categoryRepository;

  public async Task<Result> Handle(Command request, CancellationToken cancellationToken)
  {
    var category = await _categoriesRepository.GetByPublicIdAsync(request.PublicId, cancellationToken);

    if (category is null)
    {
      return Result.Failure(CategoryErrors.NotFound(request.PublicId));
    }

    if (request.Name is not null)
    {
      var renameResult = category.Rename(request.Name);
      if (renameResult.IsFailure)
      {
        return renameResult;
      }
    }

    if (request.Description is not null)
    {
      var updateDescriptionResult = category.ChangeDescription(request.Description);
      if (updateDescriptionResult.IsFailure)
      {
        return updateDescriptionResult;
      }
    }

    _categoriesRepository.Update(category);

    return Result.Success();
  }
}
