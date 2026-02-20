using ScreenDrafts.Modules.Drafts.Domain.Participants;

namespace ScreenDrafts.Modules.Drafts.Domain.Drafters.DomainEvents;

public sealed class VetoOverrideCreatedDomainEvent(
  Guid vetoOverrideId,
  Guid vetoId,
  Participant issuedBy) : DomainEvent
{
  public Guid VetoOverrideId { get; init; } = vetoOverrideId;
  public Participant IssuedBy { get; init; } = issuedBy;
  public Guid VetoId { get; init; } = vetoId;
}
