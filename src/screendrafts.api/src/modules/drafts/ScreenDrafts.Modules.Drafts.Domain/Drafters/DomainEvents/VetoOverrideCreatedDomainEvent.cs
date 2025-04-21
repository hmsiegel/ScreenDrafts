namespace ScreenDrafts.Modules.Drafts.Domain.Drafters.DomainEvents;

public sealed class VetoOverrideCreatedDomainEvent(
  Guid vetoOverrideId,
  Guid? drafterId,
  Guid? drafterTeamId,
  Guid vetoId) : DomainEvent
{
  public Guid VetoOverrideId { get; init; } = vetoOverrideId;
  public Guid? DrafterId { get; init; } = drafterId;
  public Guid? DrafterTeamId { get; init; } = drafterTeamId;
  public Guid VetoId { get; init; } = vetoId;
}
