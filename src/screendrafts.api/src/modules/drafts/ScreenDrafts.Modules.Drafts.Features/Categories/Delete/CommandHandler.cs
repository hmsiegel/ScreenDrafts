namespace ScreenDrafts.Modules.Drafts.Features.Categories.Delete;

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

    if (category.IsDeleted)
    {
      return Result.Success();
    }

    var deleteResult = category.SoftDelete();

    if (!deleteResult.IsFailure)
    {
      return Result.Failure(CategoryErrors.DeletionFailed(request.PublicId));
    }

    return Result.Success();
  }
}
