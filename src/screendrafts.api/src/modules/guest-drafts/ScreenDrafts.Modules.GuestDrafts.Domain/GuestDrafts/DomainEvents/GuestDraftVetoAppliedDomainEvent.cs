namespace ScreenDrafts.Modules.GuestDrafts.Domain.GuestDrafts.DomainEvents;

/// <summary>
/// VetoTokensRemaining/OverrideTokensRemaining are computed off the issuing
/// participant's in-memory state at the moment ApplyVeto succeeds -- not
/// re-queried -- so no cross-schema read is needed downstream.
/// </summary>
public sealed class GuestDraftVetoAppliedDomainEvent(
  Guid guestDraftId,
  string guestDraftPublicId,
  Guid pickId,
  int playOrder,
  string moviePublicId,
  Guid vetoedByParticipantId,
  Guid playedByParticipantId,
  int vetoTokensRemaining,
  int overrideTokensRemaining
) : DomainEvent
{
  public Guid GuestDraftId { get; init; } = guestDraftId;
  public string GuestDraftPublicId { get; init; } = guestDraftPublicId;
  public Guid PickId { get; init; } = pickId;
  public int PlayOrder { get; init; } = playOrder;
  public string MoviePublicId { get; init; } = moviePublicId;
  public Guid VetoedByParticipantId { get; init; } = vetoedByParticipantId;
  public Guid PlayedByParticipantId { get; init; } = playedByParticipantId;
  public int VetoTokensRemaining { get; init; } = vetoTokensRemaining;
  public int OverrideTokensRemaining { get; init; } = overrideTokensRemaining;
}
