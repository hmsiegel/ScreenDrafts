namespace ScreenDrafts.Modules.Drafts.Domain.Drafters.DomainEvents;

public sealed class RolloverVetoCreatedDomainEvent(
  Guid rolloverVetoId,
  Guid? drafter,
  Guid? drafterTeam,
  Guid? fromDraftId) : DomainEvent
{
  public Guid RolloverVetoId { get; init; } = rolloverVetoId;
  public Guid? Drafter { get; init; } = drafter;
  public Guid? DrafterTeam { get; init; } = drafterTeam;
  public Guid? FromDraftId { get; init; } = fromDraftId;
}

