namespace ScreenDrafts.Modules.Drafts.Domain.Categories;

public sealed record CategoryId(Guid Value)
{
  public Guid Value { get; init; } = Value;

  public static CategoryId CreateUnique() => new(Guid.NewGuid());

  public static CategoryId FromString(string value) => new(Guid.Parse(value));

  public static CategoryId Create(Guid value) => new(value);
}
