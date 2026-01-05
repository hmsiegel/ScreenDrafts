namespace ScreenDrafts.Modules.Drafts.Domain.Predictions.ValueObjects;

public sealed record PredictionCarryoverId(Guid Value)
{
  public Guid Value { get; init; } = Value;
  public static PredictionCarryoverId FromString(string value) => new(Guid.Parse(value, CultureInfo.InvariantCulture));
  public static PredictionCarryoverId Create(Guid value) => new(value);
  public static PredictionCarryoverId CreateUnique() => new(Guid.NewGuid());
}


