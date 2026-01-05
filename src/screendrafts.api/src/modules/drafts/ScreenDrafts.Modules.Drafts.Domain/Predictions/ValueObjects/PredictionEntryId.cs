namespace ScreenDrafts.Modules.Drafts.Domain.Predictions.ValueObjects;

public sealed record PredictionEntryId(Guid Value)
{
  public Guid Value { get; init; } = Value;
  public static PredictionEntryId FromString(string value) => new(Guid.Parse(value, CultureInfo.InvariantCulture));
  public static PredictionEntryId Create(Guid value) => new(value);
  public static PredictionEntryId CreateUnique() => new(Guid.NewGuid());
}
