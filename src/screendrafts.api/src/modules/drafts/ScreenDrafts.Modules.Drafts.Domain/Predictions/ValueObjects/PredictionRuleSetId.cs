namespace ScreenDrafts.Modules.Drafts.Domain.Predictions.ValueObjects;

public sealed record PredictionRuleSetId(Guid Value)
{
  public Guid Value { get; init; } = Value;
  public static PredictionRuleSetId FromString(string value) => new(Guid.Parse(value, CultureInfo.InvariantCulture));
  public static PredictionRuleSetId Create(Guid value) => new(value);
  public static PredictionRuleSetId CreateUnique() => new(Guid.NewGuid());
}


