namespace ScreenDrafts.Modules.Drafts.Domain.DraftParts.ValueObjects;

public sealed record MovieVersion : ValueObject
{
  public MovieVersion(string name)
  {
    Name = name;
  }

  private MovieVersion()
  {
  }

  public string Name { get; private set; } = null!;

  public IEnumerable<object> GetEqualityComponents()
  {
    yield return Name.ToUpperInvariant();
  }
}
