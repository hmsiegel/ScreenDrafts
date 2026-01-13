namespace ScreenDrafts.Modules.Drafts.Domain.Drafts.Entities;

public sealed class Category : Entity<CategoryId>
{
  public const int NameMaxLength = 100;
  public const int DescriptionMaxLength = 500;

  private readonly List<DraftCategory> _draftCategories = [];

  private Category(
    string publicId,
    string name,
    string? description,
    DateTime createdOnUtc,
    CategoryId? id = null)
    : base(id ?? CategoryId.CreateUnique())
  {
    PublicId = publicId;
    Name = name;
    Description = description;
    CreatedOnUtc = createdOnUtc;
  }

  private Category()
  {
  }

  public string PublicId { get; private set; } = default!;

  public string Name { get; private set; } = default!;

  public string? Description { get; private set; } = default!;

  public DateTime CreatedOnUtc { get; private set; } = default!;

  public DateTime? ModifiedOnUtc { get; private set; } = default!;

  public bool IsDeleted { get; private set; } = default!;

  public IReadOnlyCollection<DraftCategory> DraftCategories => _draftCategories.AsReadOnly();

  public static Result<Category> Create(
    string publicId,
    string name,
    string? description)
  {
    if (string.IsNullOrWhiteSpace(name))
    {
      return Result.Failure<Category>(CategoryErrors.CategoryNameIsRequired);
    }
    var category = new Category(
      publicId: publicId,
      name: name,
      description: description,
      createdOnUtc: DateTime.UtcNow);
    return Result.Success(category);
  }

  public Result Rename(string name)
  {
    if (string.IsNullOrWhiteSpace(name))
    {
      return Result.Failure(CategoryErrors.CategoryNameIsRequired);
    }
    Name = name;
    ModifiedOnUtc = DateTime.UtcNow;
    return Result.Success();
  }

  public Result ChangeDescription(string description)
  {
    if (string.IsNullOrWhiteSpace(description))
    {
      return Result.Failure(CategoryErrors.CategoryDescriptionIsRequired);
    }
    Description = description;
    ModifiedOnUtc = DateTime.UtcNow;
    return Result.Success();
  }

  public Result Delete()
  {
    if (IsDeleted)
    {
      return Result.Success();
    }

    IsDeleted = true;
    ModifiedOnUtc = DateTime.UtcNow;
    return Result.Success();
  }

  public Result Restore()
  {
    if (!IsDeleted)
    {
      return Result.Success();
    }
    IsDeleted = false;
    ModifiedOnUtc = DateTime.UtcNow;
    return Result.Success();
  }
}
