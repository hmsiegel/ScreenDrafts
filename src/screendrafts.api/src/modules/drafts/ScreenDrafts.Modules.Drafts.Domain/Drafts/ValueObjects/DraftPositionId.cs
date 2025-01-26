namespace ScreenDrafts.Modules.Drafts.Domain.Drafts.ValueObjects;

public sealed record DraftPositionId(Guid Value)
{
  public Guid Value { get; init; } = Value;

  public static DraftPositionId CreateUnique() => new(Guid.NewGuid());

  public static DraftPositionId FromString(string value) => new(Guid.Parse(value, CultureInfo.InvariantCulture));

  public static DraftPositionId Create(Guid value) => new(value);
}
