namespace ScreenDrafts.Modules.GuestDrafts.Domain.GuestDrafts.ValueObjects;

public sealed record GuestDraftParticipantId(Guid Value)
{
  public Guid Value { get; init; } = Value;

  public static GuestDraftParticipantId CreateUnique() => new(Guid.NewGuid());

  public static GuestDraftParticipantId FromString(string value) => new(Guid.Parse(value));

  public static GuestDraftParticipantId Create(Guid value) => new(value);
}
