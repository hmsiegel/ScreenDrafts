namespace ScreenDrafts.Modules.Drafts.IntegrationEvents;

public sealed class VetoOverrideAppliedIntegrationEvent(
  Guid id,
  DateTime occurredOnUtc,
  Guid draftPartId,
  string draftPartPublicId,
  int playOrder,
  int tmdbId,
  string movieTitle,
  Guid overriddenByParticipantId,
  int overriddenByParticipantKind
) : IntegrationEvent(id, occurredOnUtc)
{
  public Guid DraftPartId { get; init; } = draftPartId;
  public string DraftPartPublicId { get; init; } = draftPartPublicId;
  public int PlayOrder { get; init; } = playOrder;
  public int TmdbId { get; init; } = tmdbId;
  public string MovieTitle { get; init; } = movieTitle;
  public Guid OverriddenByParticipantId { get; init; } = overriddenByParticipantId;
  public int OverriddenByParticipantKind { get; init; } = overriddenByParticipantKind;
}
