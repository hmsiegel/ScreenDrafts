namespace ScreenDrafts.Modules.Drafts.Domain.Drafters.DomainEvents;

public sealed class DrafterTeamCreatedDomainEvent(Guid drafterTeamId) : DomainEvent
{
  public Guid DrafterTeamId { get; init; } = drafterTeamId;
}
