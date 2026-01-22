namespace ScreenDrafts.Modules.Drafts.Domain.Drafters.DomainEvents;

public sealed class VetoOverrideCreatedDomainEvent(
  Guid vetoOverrideId,
  Guid vetoId,
  ParticipantId issuedBy) : DomainEvent
{
  public Guid VetoOverrideId { get; init; } = vetoOverrideId;
  public ParticipantId IssuedBy { get; init; } = issuedBy;
  public Guid VetoId { get; init; } = vetoId;
}
