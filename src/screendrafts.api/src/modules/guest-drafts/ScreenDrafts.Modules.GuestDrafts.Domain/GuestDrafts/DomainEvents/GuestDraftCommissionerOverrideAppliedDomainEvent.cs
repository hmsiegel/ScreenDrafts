namespace ScreenDrafts.Modules.GuestDrafts.Domain.GuestDrafts.DomainEvents;

public sealed class GuestDraftCommissionerOverrideAppliedDomainEvent(
  Guid guestDraftId,
  string guestDraftPublicId,
  Guid pickId,
  int playOrder,
  int boardPosition,
  string moviePublicId,
  Guid playedByParticipantId,
  int vetoTokensRemaining,
  int overrideTokensRemaining
) : DomainEvent
{
  public Guid GuestDraftId { get; init; } = guestDraftId;
  public string GuestDraftPublicId { get; init; } = guestDraftPublicId;
  public Guid PickId { get; init; } = pickId;
  public int PlayOrder { get; init; } = playOrder;
  public int BoardPosition { get; init; } = boardPosition;
  public string MoviePublicId { get; init; } = moviePublicId;
  public Guid PlayedByParticipantId { get; init; } = playedByParticipantId;
  public int VetoTokensRemaining { get; init; } = vetoTokensRemaining;
  public int OverrideTokensRemaining { get; init; } = overrideTokensRemaining;
}
