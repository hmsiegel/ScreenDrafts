namespace ScreenDrafts.Modules.Drafts.IntegrationEvents;

public sealed class DraftPartReleaseAddedIntegrationEvent(
  Guid id,
  DateTime occurredOnUtc,
  Guid draftId,
  string draftPublicId,
  string draftPartPublicId,
  string releaseChannel,
  DateOnly releaseDate,
  int? episodeNumber
) : IntegrationEvent(id, occurredOnUtc)
{
  public Guid DraftId { get; set; } = draftId;
  public string DraftPublicId { get; set; } = draftPublicId;
  public string DraftPartPublicId { get; set; } = draftPartPublicId;
  public string ReleaseChannel { get; set; } = releaseChannel;
  public DateOnly ReleaseDate { get; set; } = releaseDate;
  public int? EpisodeNumber { get; set; } = episodeNumber;
}
