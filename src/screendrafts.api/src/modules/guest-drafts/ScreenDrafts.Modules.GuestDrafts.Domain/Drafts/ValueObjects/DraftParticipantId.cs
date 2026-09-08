namespace ScreenDrafts.Modules.GuestDrafts.Domain.Drafts.ValueObjects;

public sealed record DraftParticipantId(Guid Value)
{
  public Guid Value { get; init; } = Value;

  public static DraftParticipantId CreateUnique() => new(Guid.NewGuid());

  public static DraftParticipantId FromString(string value) => new(Guid.Parse(value));

  public static DraftParticipantId Create(Guid value) => new(value);
}
