namespace ScreenDrafts.Modules.Drafts.Domain.Predictions.ValueObjects;

public sealed record PredictionStandingId(Guid Value)
{
  public Guid Value { get; init; } = Value;
  public static PredictionStandingId FromString(string value) => new(Guid.Parse(value, CultureInfo.InvariantCulture));
  public static PredictionStandingId Create(Guid value) => new(value);
  public static PredictionStandingId CreateUnique() => new(Guid.NewGuid());
}


