namespace ScreenDrafts.Modules.Drafts.Domain.Campaigns;

public sealed class Campaign : Entity
{
  public const int MaxNameLength = 200;
  private readonly List<Draft> _drafts = [];
  private Campaign(
    string slug,
    string name,
    string publicId,
    Guid? id = null)
    : base(id ?? Guid.NewGuid())
  {
    Slug = Guard.Against.NullOrEmpty(slug);
    Name = Guard.Against.NullOrEmpty(name);
    PublicId = Guard.Against.NullOrEmpty(publicId);
  }

  private Campaign()
  {

  }

  public string PublicId { get; private set; } = default!;
  public string Slug { get; private set; } = default!;
  public string Name { get; private set; } = default!;
  public DateTime CreatedAtUtc { get; private set; } = DateTime.UtcNow;
  public DateTime? UpdatedAtUtc { get; private set; } = default!;
  public bool IsDeleted { get; private set; } = default!;

  public IReadOnlyCollection<Draft> Drafts => _drafts.AsReadOnly();

  public static Result<Campaign> Create(
    string slug,
    string name,
    string publicId,
    Guid? id = null)
  {
    var campaign = new Campaign(
      slug: slug,
      name: name,
      publicId: publicId,
      id: id);
    return Result.Success(campaign);
  }

  public Result Rename(string name)
  {
    Name = Guard.Against.NullOrEmpty(name);
    UpdatedAtUtc = DateTime.UtcNow;
    return Result.Success();
  }

  public Result ChangeSlug(string slug)
  {
    Slug = Guard.Against.NullOrEmpty(slug);
    UpdatedAtUtc = DateTime.UtcNow;
    return Result.Success();
  }

  public Result SoftDelete()
  {
    if (IsDeleted)
    {
      return Result.Success();
    }

    IsDeleted = true;
    UpdatedAtUtc = DateTime.UtcNow;
    return Result.Success();
  }

  public Result Restore()
  {
    if (!IsDeleted)
    {
      return Result.Success();
    }
    IsDeleted = false;
    UpdatedAtUtc = DateTime.UtcNow;
    return Result.Success();
  }
}
