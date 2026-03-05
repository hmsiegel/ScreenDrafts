namespace ScreenDrafts.Modules.Drafts.Domain.DraftParts.DomainEvents;

public sealed class VetoOverrideAddedDomainEvent(Guid draftPartId) : DomainEvent
{
  public Guid DraftPartId { get; init; } = draftPartId;
}
