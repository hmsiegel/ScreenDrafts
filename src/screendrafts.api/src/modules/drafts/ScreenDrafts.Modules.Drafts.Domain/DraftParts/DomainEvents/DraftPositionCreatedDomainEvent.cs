namespace ScreenDrafts.Modules.Drafts.Domain.DraftParts.DomainEvents;

public sealed class DraftPositionCreatedDomainEvent(Guid draftPositionId, string publicId) : DomainEvent
{
  public Guid DraftPositionId { get; } = draftPositionId;
  public string PublicId { get; } = publicId;
}
