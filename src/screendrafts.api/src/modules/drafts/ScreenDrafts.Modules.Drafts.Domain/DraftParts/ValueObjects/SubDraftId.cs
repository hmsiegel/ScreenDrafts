namespace ScreenDrafts.Modules.Drafts.Domain.DraftParts.ValueObjects;

public sealed record SubDraftId(Guid Value)
{
  public Guid Value { get; init; } = Value;
  public static SubDraftId CreateUnique() => new(Guid.NewGuid());
  public static SubDraftId Create(Guid value) => new(value);
}
