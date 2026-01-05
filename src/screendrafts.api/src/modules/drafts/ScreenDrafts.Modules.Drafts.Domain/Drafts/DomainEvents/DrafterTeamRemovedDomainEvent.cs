namespace ScreenDrafts.Modules.Drafts.Domain.Drafts.DomainEvents;

public sealed class DrafterTeamRemovedDomainEvent(Guid draftId, Guid drafterTeamId) : DomainEvent
{
  public Guid DraftId { get; init; } = draftId;
  public Guid DrafterTeamId { get; init; } = drafterTeamId;
}
