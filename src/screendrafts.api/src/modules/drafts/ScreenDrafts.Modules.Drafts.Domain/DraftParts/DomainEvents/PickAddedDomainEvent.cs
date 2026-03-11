namespace ScreenDrafts.Modules.Drafts.Domain.DraftParts.DomainEvents;

public sealed class PickAddedDomainEvent(
  Guid draftPartId,
  string draftPartPublicId,
  string imdbId,
  int? tmdbId,
  string movieTitle,
  Guid participantId,
  int participantKind) : DomainEvent
{
  public Guid DraftPartId { get; init; } = draftPartId;
  public string DraftPartPublicId { get; init; } = draftPartPublicId;
  public string ImdbId { get; init; } = imdbId;
  public int? TmdbId { get; init; } = tmdbId;
  public string MovieTitle { get; init; } = movieTitle;
  public Guid ParticipantId { get; init; } = participantId;
  public int ParticipantKind { get; init; } = participantKind;
}
