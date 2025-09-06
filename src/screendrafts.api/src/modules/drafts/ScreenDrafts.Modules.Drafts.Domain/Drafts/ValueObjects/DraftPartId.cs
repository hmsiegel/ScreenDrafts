namespace ScreenDrafts.Modules.Drafts.Domain.Drafts.ValueObjects;

public sealed record DraftPartId(Guid Value)
{
  public Guid Value { get; init; } = Value;
  public static DraftPartId CreateUnique() => new(Guid.NewGuid());
  public static DraftPartId FromString(string value) => new(Guid.Parse(value));
  public static DraftPartId Create(Guid value) => new(value);
}
