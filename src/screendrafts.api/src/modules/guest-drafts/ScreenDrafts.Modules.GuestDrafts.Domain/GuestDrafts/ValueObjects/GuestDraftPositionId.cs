namespace ScreenDrafts.Modules.GuestDrafts.Domain.GuestDrafts.ValueObjects;

public sealed record GuestDraftPositionId(Guid Value)
{
  public Guid Value { get; init; } = Value;

  public static GuestDraftPositionId CreateUnique() => new(Guid.NewGuid());

  public static GuestDraftPositionId FromString(string value) => new(Guid.Parse(value));

  public static GuestDraftPositionId Create(Guid value) => new(value);
}
