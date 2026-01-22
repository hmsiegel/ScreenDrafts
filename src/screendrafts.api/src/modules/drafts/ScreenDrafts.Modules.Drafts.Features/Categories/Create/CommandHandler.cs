namespace ScreenDrafts.Modules.Drafts.Features.Categories.Create;

internal sealed class CommandHandler(ICategoriesRepository categoryRepository, IPublicIdGenerator publicIdGenerator)
  : Common.Features.Abstractions.Messaging.ICommandHandler<Command, string>
{
  private readonly ICategoriesRepository _categoryRepository = categoryRepository;
  private readonly IPublicIdGenerator _publicIdGenerator = publicIdGenerator;

  public async Task<Result<string>> Handle(Command command, CancellationToken cancellationToken)
  {
    var publicId = _publicIdGenerator.GeneratePublicId(PublicIdPrefixes.Category);

    // Check for uniqueness of slug
    if (await _categoryRepository.ExistsByNameAsync(command.Name, cancellationToken))
    {
      return Result.Failure<string>(CategoryErrors.DuplicateName(command.Name));
    }

    var category = Category.Create(publicId, command.Name, publicId);

    _categoryRepository.Add(category.Value);

    return Result.Success(category.Value.PublicId);
  }
}


