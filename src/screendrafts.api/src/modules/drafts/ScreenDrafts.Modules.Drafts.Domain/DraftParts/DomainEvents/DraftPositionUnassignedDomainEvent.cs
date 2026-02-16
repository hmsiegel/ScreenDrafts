namespace ScreenDrafts.Modules.Drafts.Domain.DraftParts.DomainEvents;

public sealed class DraftPositionUnassignedDomainEvent(
  Guid draftPartId,
  Guid draftPositionId) : DomainEvent
{
  public Guid DraftPartId { get; init; } = draftPartId;
  public Guid DraftPositionId { get; init; } = draftPositionId;
}
