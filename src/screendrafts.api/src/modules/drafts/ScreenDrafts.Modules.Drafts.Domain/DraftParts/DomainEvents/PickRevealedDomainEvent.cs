namespace ScreenDrafts.Modules.Drafts.Domain.DraftParts.DomainEvents;

public sealed class PickRevealedDomainEvent(
  Guid draftPartId,
  string draftPartPublicId,
  Guid pickId,
  int playOrder,
  Guid movieId,
  string? actedByPublicId,
  Guid draftId,
  string draftPublicId,
  int canonicalPolicyValue,
  Guid? subDraftId = null,
  string? subDraftPublicId = null
) : DomainEvent
{
  public Guid DraftPartId { get; init; } = draftPartId;
  public string DraftPartPublicId { get; init; } = draftPartPublicId;
  public Guid PickId { get; init; } = pickId;
  public int PlayOrder { get; init; } = playOrder;
  public Guid MovieId { get; init; } = movieId;
  public string? ActedByPublicId { get; init; } = actedByPublicId;
  public Guid DraftId { get; init; } = draftId;
  public string DraftPublicId { get; init; } = draftPublicId;
  public int CanonicalPolicyValue { get; init; } = canonicalPolicyValue;
  public Guid? SubDraftId { get; init; } = subDraftId;
  public string? SubDraftPublicId { get; init; } = subDraftPublicId;
}
