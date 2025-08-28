namespace ScreenDrafts.Modules.Drafts.Domain.Drafts.Entities;
public sealed class Category : Entity<CategoryId>
{
  public const int NameMaxLength = 100;
  public const int DescriptionMaxLength = 500;

  private readonly List<DraftCategory> _draftCategories = [];

  private Category(
    string name,
    string? description,
    DateTime createdOnUtc,
    CategoryId? id = null)
    : base(id ?? CategoryId.CreateUnique())
  {
    Name = name;
    Description = description;
    CreatedOnUtc = createdOnUtc;
  }

  private Category()
  {
  }

  public string Name { get; private set; } = default!;

  public string? Description { get; private set; } = default!;

  public DateTime CreatedOnUtc { get; private set; } = default!;

  public DateTime? ModifiedOnUtc { get; private set; } = default!;

  public IReadOnlyCollection<DraftCategory> DraftCategories => _draftCategories.AsReadOnly();

  public static Result<Category> Create(
    string name,
    string? description)
  {
    if (string.IsNullOrWhiteSpace(name))
    {
      return Result.Failure<Category>(CategoryErrors.CategoryNameIsRequired);
    }
    var category = new Category(
      name: name,
      description: description,
      createdOnUtc: DateTime.UtcNow);
    return Result.Success(category);
  }
}
