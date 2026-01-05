namespace ScreenDrafts.Modules.Drafts.Domain.Predictions.ValueObjects;

public sealed record DraftPredictionSetId(Guid Value)
{
  public Guid Value { get; init; } = Value;
  public static DraftPredictionSetId FromString(string value) => new(Guid.Parse(value, CultureInfo.InvariantCulture));
  public static DraftPredictionSetId Create(Guid value) => new(value);
  public static DraftPredictionSetId CreateUnique() => new(Guid.NewGuid());
}
