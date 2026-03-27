namespace ScreenDrafts.Modules.Drafts.Domain.DraftParts.DomainEvents;

public sealed class PickAddedDomainEvent(
  Guid draftPartId,
  string draftPartPublicId,
  string? imdbId,
  int? tmdbId,
  string movieTitle,
  Guid participantId,
  int participantKind,
  Guid draftId,
  string draftPublicId,
  int canonicalPolicyValue,
  string moviePublicId,
  int boardPosition) : DomainEvent
{
  public Guid DraftPartId { get; init; } = draftPartId;
  public string DraftPartPublicId { get; init; } = draftPartPublicId;
  public Guid DraftId { get; init; } = draftId;
  public string DraftPublicId { get; init; } = draftPublicId;
  public string? ImdbId { get; init; } = imdbId;
  public int? TmdbId { get; init; } = tmdbId;
  public string MovieTitle { get; init; } = movieTitle;
  public string MoviePublicId { get; init; } = moviePublicId;
  public int BoardPosition { get; init; } = boardPosition;
  public Guid ParticipantId { get; init; } = participantId;
  public int ParticipantKind { get; init; } = participantKind;
  public int CanonicalPolicyValue { get; init; } = canonicalPolicyValue;
}
