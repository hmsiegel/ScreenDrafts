using ScreenDrafts.Modules.Drafts.Domain.Participants;

namespace ScreenDrafts.Modules.Drafts.Domain.Drafters.DomainEvents;

public sealed class VetoCreatedDomainEvent(
  Guid vetoId,
  Guid pickId,
  Participant? issuedBy) : DomainEvent
{
  public Guid VetoId { get; init; } = vetoId;
  public Guid PickId { get; init; } = pickId;
  public Participant? IssuedBy { get; init; } = issuedBy;
}
