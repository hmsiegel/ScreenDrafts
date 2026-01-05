namespace ScreenDrafts.Modules.Drafts.Domain.Drafters.DomainEvents;

public sealed class VetoCreatedDomainEvent(
  Guid vetoId,
  Guid pickId,
  ParticipantId? issuedBy) : DomainEvent
{
  public Guid VetoId { get; init; } = vetoId;
  public Guid PickId { get; init; } = pickId;
  public ParticipantId? IssuedBy { get; init; } = issuedBy;
}
