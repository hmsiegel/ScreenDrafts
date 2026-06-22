namespace ScreenDrafts.Modules.Drafts.IntegrationEvents;

public sealed class CommissionerOverrideAppliedIntegrationEvent(
  Guid id,
  DateTime occurredOnUtc,
  Guid draftPartId,
  string draftPartPublicId,
  int tmdbId,
  string movieTitle,
  Guid participantId,
  int participantKind,
  int boardPosition
) : IntegrationEvent(id, occurredOnUtc)
{
  public Guid DraftPartId { get; init; } = draftPartId;
  public string DraftPartPublicId { get; init; } = draftPartPublicId;
  public int TmdbId { get; init; } = tmdbId;
  public string MovieTitle { get; init; } = movieTitle;
  public Guid ParticipantId { get; init; } = participantId;
  public int ParticipantKind { get; init; } = participantKind;
  public int BoardPosition { get; init; } = boardPosition;
}
