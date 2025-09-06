namespace ScreenDrafts.Modules.Drafts.Domain.Drafts.Entities;

public sealed class Campaign  : Entity
{
  public Campaign(
    string slug,
    string name,
    Guid? id = null)
    : base(id ?? Guid.NewGuid())
  {
    Slug = Guard.Against.NullOrEmpty(slug);
    Name = Guard.Against.NullOrEmpty(name);
  }

  public Campaign()
  {

  }

  public string Slug { get; private set; } = default!;
  public string Name { get; private set; } = default!;
}
