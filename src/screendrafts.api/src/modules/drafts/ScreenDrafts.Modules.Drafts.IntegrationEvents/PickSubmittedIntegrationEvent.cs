namespace ScreenDrafts.Modules.Drafts.IntegrationEvents;

public sealed class PickSubmittedIntegrationEvent(
  Guid id,
  DateTime occurredOnUtc,
  Guid draftPartId,
  string draftPartPublicId,
  int playOrder,
  Guid movieId,
  string moviePublicId,
  string movieTitle,
  string? imdbId,
  int? tmdbId,
  int boardPosition,
  Guid participantId,
  int participantKind,
  string? actedByPublicId)
  : IntegrationEvent(id, occurredOnUtc)
{
  public Guid DraftPartId { get; set; } = draftPartId;
  public string DraftPartPublicId { get; set; } = draftPartPublicId;
  public int PlayOrder { get; set; } = playOrder;
  public Guid MovieId { get; set; } = movieId;
  public string MoviePublicId { get; set; } = moviePublicId;
  public string MovieTitle { get; set; } = movieTitle;
  public string? ImdbId { get; set; } = imdbId;
  public int? TmdbId { get; set; } = tmdbId;
  public int BoardPosition { get; set; } = boardPosition;
  public Guid ParticipantId { get; set; } = participantId;
  public int ParticipantKind { get; set; } = participantKind;
  public string? ActedByPublicId { get; set; } = actedByPublicId;
}
