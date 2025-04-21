namespace ScreenDrafts.Modules.Drafts.Domain.Drafters.DomainEvents;

public sealed class VetoCreatedDomainEvent(
  Guid vetoId,
  Guid? drafterId,
  Guid? drafterTeamId,
  Guid pickId) : DomainEvent
{
  public Guid VetoId { get; init; } = vetoId;
  public Guid? DrafterId { get; init; } = drafterId;
  public Guid? DrafterTeamId { get; init; } = drafterTeamId;
  public Guid PickId { get; init; } = pickId;
}
