namespace ScreenDrafts.Modules.GuestDrafts.Domain.Drafts.DomainEvents;

public sealed class VetoOverriddenDomainEvent(
  Guid guestDraftId,
  string guestDraftPublicId,
  Guid pickId,
  int playOrder,
  string moviePublicId,
  Guid overriddenByParticipantId,
  int vetoTokensRemaining,
  int overrideTokensRemaining
) : DomainEvent
{
  public Guid GuestDraftId { get; init; } = guestDraftId;
  public string GuestDraftPublicId { get; init; } = guestDraftPublicId;
  public Guid PickId { get; init; } = pickId;
  public int PlayOrder { get; init; } = playOrder;
  public string MoviePublicId { get; init; } = moviePublicId;
  public Guid OverriddenByParticipantId { get; init; } = overriddenByParticipantId;
  public int VetoTokensRemaining { get; init; } = vetoTokensRemaining;
  public int OverrideTokensRemaining { get; init; } = overrideTokensRemaining;
}
