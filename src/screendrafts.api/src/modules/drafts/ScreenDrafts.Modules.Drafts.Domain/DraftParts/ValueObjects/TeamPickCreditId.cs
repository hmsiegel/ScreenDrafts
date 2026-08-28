namespace ScreenDrafts.Modules.Drafts.Domain.DraftParts.ValueObjects;

public sealed record TeamPickCreditId(Guid Value)
{
  public Guid Value { get; init; } = Value;

  public static TeamPickCreditId CreateUnique() => new(Guid.NewGuid());

  public static TeamPickCreditId Create(Guid value) => new(value);
}
