namespace ScreenDrafts.Modules.Drafts.Domain.DraftParts.ValueObjects;

public sealed record DraftPartParticipantId(Guid Value)
{
  public Guid Value { get; init; } = Value;
  public static DraftPartParticipantId CreateUnique() => new(Guid.NewGuid());
  public static DraftPartParticipantId FromString(string value) => new(Guid.Parse(value));
  public static DraftPartParticipantId Create(Guid value) => new(value);
}
