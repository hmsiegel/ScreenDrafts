namespace ScreenDrafts.Modules.GuestDrafts.Domain.Drafts.ValueObjects;

public sealed record DraftPositionId(Guid Value)
{
  public Guid Value { get; init; } = Value;

  public static DraftPositionId CreateUnique() => new(Guid.NewGuid());

  public static DraftPositionId FromString(string value) => new(Guid.Parse(value));

  public static DraftPositionId Create(Guid value) => new(value);
}
