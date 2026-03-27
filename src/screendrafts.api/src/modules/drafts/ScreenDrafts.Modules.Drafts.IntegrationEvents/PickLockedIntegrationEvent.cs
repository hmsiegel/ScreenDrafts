using ScreenDrafts.Common.Application.EventBus;

namespace ScreenDrafts.Modules.Drafts.IntegrationEvents;

public sealed class PickLockedIntegrationEvent(
  Guid id,
  DateTime occurredOnUtc,
  Guid draftPartId,
  string draftPartPublicId,
  Guid draftId,
  string draftPublicId,
  string moviePublicId,
  string movieTitle,
  int? tmdbId,
  int boardPosition,
  Guid participantIdValue,
  int participantKindValue,
  int canonicalPolicyValue,
  bool hasMainFeedRelease)
  : IntegrationEvent(id, occurredOnUtc)
{
  public Guid DraftPartId { get; set; } = draftPartId;
  public string DraftPartPublicId { get; set; } = draftPartPublicId;
  public Guid DraftId { get; set; } = draftId;
  public string DraftPublicId { get; set; } = draftPublicId;
  public string MoviePublicId { get; set; } = moviePublicId;
  public string MovieTitle { get; set; } = movieTitle;
  public int? TmdbId { get; set; } = tmdbId;
  public int BoardPosition { get; set; } = boardPosition;
  public Guid ParticipantIdValue { get; set; } = participantIdValue;
  public int ParticipantKindValue { get; set; } = participantKindValue;
  public int CanonicalPolicyValue { get; set; } = canonicalPolicyValue;
  public bool HasMainFeedRelease { get; set; } = hasMainFeedRelease;
}
