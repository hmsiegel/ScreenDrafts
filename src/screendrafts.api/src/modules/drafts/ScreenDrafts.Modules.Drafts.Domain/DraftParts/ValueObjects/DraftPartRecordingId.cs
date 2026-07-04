namespace ScreenDrafts.Modules.Drafts.Domain.DraftParts.ValueObjects;

public sealed record DraftPartRecordingId(Guid Value)
{
  public Guid Value { get; init; } = Value;

  public static DraftPartRecordingId CreateUnique() => new(Guid.NewGuid());

  public static DraftPartRecordingId FromString(string value) => new(Guid.Parse(value));

  public static DraftPartRecordingId Create(Guid value) => new(value);
}
