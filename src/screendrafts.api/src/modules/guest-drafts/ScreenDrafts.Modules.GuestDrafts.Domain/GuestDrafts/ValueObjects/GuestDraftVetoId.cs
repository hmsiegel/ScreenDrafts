namespace ScreenDrafts.Modules.GuestDrafts.Domain.GuestDrafts.ValueObjects;

public sealed record GuestDraftVetoId(Guid Value)
{
  public Guid Value { get; init; } = Value;

  public static GuestDraftVetoId CreateUnique() => new(Guid.NewGuid());

  public static GuestDraftVetoId FromString(string value) => new(Guid.Parse(value));

  public static GuestDraftVetoId Create(Guid value) => new(value);
}
