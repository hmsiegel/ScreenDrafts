namespace ScreenDrafts.Modules.Drafts.Features.Categories.Create;

internal sealed class CreateCategoryCommandHandler(ICategoryRepository categoryRepository, IPublicIdGenerator publicIdGenerator)
  : ICommandHandler<CreateCategoryCommand, string>
{
  private readonly ICategoryRepository _categoryRepository = categoryRepository;
  private readonly IPublicIdGenerator _publicIdGenerator = publicIdGenerator;

  public async Task<Result<string>> Handle(CreateCategoryCommand CreateCategoryCommand, CancellationToken cancellationToken)
  {
    var publicId = _publicIdGenerator.GeneratePublicId(PublicIdPrefixes.Category);

    // Check for uniqueness of slug
    if (await _categoryRepository.ExistsByNameAsync(CreateCategoryCommand.Name, cancellationToken))
    {
      return Result.Failure<string>(CategoryErrors.DuplicateName(CreateCategoryCommand.Name));
    }

    var category = Category.Create(publicId, CreateCategoryCommand.Name, publicId);

    _categoryRepository.Add(category.Value);

    return Result.Success(category.Value.PublicId);
  }
}




