namespace ScreenDrafts.Modules.Drafts.IntegrationEvents;

public sealed class VetoAppliedIntegrationEvent(
  Guid id,
  DateTime occurredOnUtc,
  Guid draftPartId,
  string draftPartPublicId,
  int playOrder,
  int tmdbId,
  string movieTitle,
  Guid vetoedByParticipantId,
  int vetoedByParticipantKind,
  Guid playedByParticipantId,
  int playedByParticipantKind
) : IntegrationEvent(id, occurredOnUtc)
{
  public Guid DraftPartId { get; init; } = draftPartId;
  public string DraftPartPublicId { get; init; } = draftPartPublicId;
  public int PlayOrder { get; init; } = playOrder;
  public int TmdbId { get; init; } = tmdbId;
  public string MovieTitle { get; init; } = movieTitle;
  public Guid VetoedByParticipantId { get; init; } = vetoedByParticipantId;
  public int VetoedByParticipantKind { get; init; } = vetoedByParticipantKind;
  public Guid PlayedByParticipantId { get; init; } = playedByParticipantId;
  public int PlayedByParticipantKind { get; init; } = playedByParticipantKind;
}
