namespace ScreenDrafts.Modules.Drafts.Domain.Drafts.DomainEvents;

public sealed class DrafterTeamAddedDomainEvent(Guid draftId, Guid drafterTeamId) : DomainEvent
{
  public Guid DraftId { get; init; } = draftId;

  public Guid DrafterTeamId { get; init; } = drafterTeamId;
}
