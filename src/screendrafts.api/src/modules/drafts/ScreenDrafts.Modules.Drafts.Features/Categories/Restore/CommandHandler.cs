namespace ScreenDrafts.Modules.Drafts.Features.Categories.Restore;

internal sealed class CommandHandler(ICategoriesRepository categoriesRepository)
  : Common.Features.Abstractions.Messaging.ICommandHandler<Command>
{
  private readonly ICategoriesRepository _categoriesRepository = categoriesRepository;

  public async Task<Result> Handle(Command request, CancellationToken cancellationToken)
  {
    var category = await _categoriesRepository.GetByPublicIdAsync(request.PublicId, cancellationToken);

    if (category is null)
    {
      return Result.Failure(CategoryErrors.NotFound(request.PublicId));
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
