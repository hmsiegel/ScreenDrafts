namespace ScreenDrafts.Modules.Drafts.Domain.Predictions.ValueObjects;

public sealed record SurrogateAssignmentId(Guid Value)
{
  public Guid Value { get; init; } = Value;
  public static SurrogateAssignmentId FromString(string value) => new(Guid.Parse(value, CultureInfo.InvariantCulture));
  public static SurrogateAssignmentId Create(Guid value) => new(value);
  public static SurrogateAssignmentId CreateUnique() => new(Guid.NewGuid());
}


