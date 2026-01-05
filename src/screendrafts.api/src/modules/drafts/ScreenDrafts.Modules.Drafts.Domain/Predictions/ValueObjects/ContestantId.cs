namespace ScreenDrafts.Modules.Drafts.Domain.Predictions.ValueObjects;

public sealed record ContestantId(Guid Value)
{
  public Guid Value { get; init; } = Value;

  public static ContestantId Create(Guid value) => new(value);
  public static ContestantId FromString(string value) => new(Guid.Parse(value, CultureInfo.InvariantCulture));
  public static ContestantId CreateUnique() => new(Guid.NewGuid());
}
