namespace ScreenDrafts.Modules.Drafts.Domain.DraftPools;

public sealed class DraftPoolLockedDomainEvent(Guid draftPoolId, Guid draftId) : DomainEvent
{
  public Guid DraftPoolId { get; init; } = draftPoolId;
  public Guid DraftId { get; init; } = draftId;
}
