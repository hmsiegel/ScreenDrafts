namespace ScreenDrafts.Modules.Drafts.Domain.Predictions.ValueObjects;

public sealed record DraftPartPredictionRulesId(Guid Value)
{
  public Guid Value { get; init; } = Value;

  public static DraftPartPredictionRulesId CreateUnique() => new(Guid.NewGuid());
  public override string ToString() => Value.ToString();
  public static DraftPartPredictionRulesId Create(Guid value) => new(value);
  public static DraftPartPredictionRulesId FromString(string value) => new(Guid.Parse(value, CultureInfo.InvariantCulture));
}
