namespace ScreenDrafts.Modules.Drafts.Application.Drafts.Commands.AddCategory;

internal sealed class CreateCategoryCommandHandler(ICategoriesRepository categoriesRepository) : ICommandHandler<CreateCategoryCommand, Guid>
{
  private readonly ICategoriesRepository _categoriesRepository = categoriesRepository;

  public async Task<Result<Guid>> Handle(CreateCategoryCommand request, CancellationToken cancellationToken)
  {
    var category = Category.Create(request.Name, request.Description);

    if (category.IsFailure)
    {
      return await Task.FromResult(Result.Failure<Guid>(category.Error!));
    }

    _categoriesRepository.AddCategory(category.Value);

    return category.Value.Id.Value;
  }
}
