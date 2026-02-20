namespace ScreenDrafts.Modules.Drafts.Domain.DrafterTeams;

public sealed class DrafterTeamCreatedDomainEvent(Guid drafterTeamId) : DomainEvent
{
  public Guid DrafterTeamId { get; init; } = drafterTeamId;
}
