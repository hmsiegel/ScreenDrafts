using ScreenDrafts.Modules.Drafts.Domain.Participants;

namespace ScreenDrafts.Modules.Drafts.Domain.Drafters.DomainEvents;

public sealed class VetoOverriddenDomainEvent(Guid value1, Guid value2, Participant? by) : DomainEvent
{
  public Guid VetoId { get; } = value1;
  public Guid PickId { get; } = value2;
  public Participant? By { get; } = by;
}
