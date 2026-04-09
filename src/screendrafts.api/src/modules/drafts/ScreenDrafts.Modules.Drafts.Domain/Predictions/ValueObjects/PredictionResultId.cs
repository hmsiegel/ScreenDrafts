namespace ScreenDrafts.Modules.Drafts.Domain.Predictions.ValueObjects;

public sealed record PredictionResultId(Guid Value)
{
  public Guid Value { get; init; } = Value;
  public static PredictionResultId FromString(string value) => new(Guid.Parse(value, CultureInfo.InvariantCulture));
  public static PredictionResultId Create(Guid value) => new(value);
  public static PredictionResultId CreateUnique() => new(Guid.NewGuid());
}
