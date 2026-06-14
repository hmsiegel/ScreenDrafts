namespace ScreenDrafts.Modules.Drafts.Domain.DraftParts.DomainEvents;

public sealed class AllPositionsAssignedDomainEvent(Guid DraftPartId, string DraftPartPublicId)
  : DomainEvent
{
  public Guid DraftPartId { get; } = DraftPartId;
  public string DraftPartPublicId { get; } = DraftPartPublicId;
}
