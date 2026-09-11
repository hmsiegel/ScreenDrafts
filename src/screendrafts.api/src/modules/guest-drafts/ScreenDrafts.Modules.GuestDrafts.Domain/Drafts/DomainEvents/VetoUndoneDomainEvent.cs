namespace ScreenDrafts.Modules.GuestDrafts.Domain.Drafts.DomainEvents;

/// <summary>
/// Token fields are nullable -- GuestDraft.UndoVeto resolves the refunded
/// issuer via FindParticipant (not GetParticipantRequired), so the issuer can
/// theoretically be null if the participant was somehow removed after issuing
/// the veto. Consumers should treat a null pair as "no token update available,"
/// not zero.
/// </summary>
public sealed class VetoUndoneDomainEvent(
  Guid guestDraftId,
  string guestDraftPublicId,
  Guid pickId,
  int playOrder,
  string moviePublicId,
  Guid? refundedToParticipantId,
  int? vetoTokensRemaining,
  int? overrideTokensRemaining
) : DomainEvent
{
  public Guid GuestDraftId { get; init; } = guestDraftId;
  public string GuestDraftPublicId { get; init; } = guestDraftPublicId;
  public Guid PickId { get; init; } = pickId;
  public int PlayOrder { get; init; } = playOrder;
  public string MoviePublicId { get; init; } = moviePublicId;
  public Guid? RefundedToParticipantId { get; init; } = refundedToParticipantId;
  public int? VetoTokensRemaining { get; init; } = vetoTokensRemaining;
  public int? OverrideTokensRemaining { get; init; } = overrideTokensRemaining;
}
