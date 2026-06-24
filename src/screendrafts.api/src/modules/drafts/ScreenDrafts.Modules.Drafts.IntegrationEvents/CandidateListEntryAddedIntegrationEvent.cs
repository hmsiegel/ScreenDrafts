namespace ScreenDrafts.Modules.Drafts.IntegrationEvents;

public sealed class CandidateListEntryAddedIntegrationEvent(
  Guid id,
  DateTime occurredOnUtc,
  Guid draftId,
  Guid draftPartId,
  int tmdbId
) : IntegrationEvent(id, occurredOnUtc)
{
  public Guid DraftId { get; init; } = draftId;
  public Guid DraftPartId { get; init; } = draftPartId;
  public int TmdbId { get; init; } = tmdbId;
}
