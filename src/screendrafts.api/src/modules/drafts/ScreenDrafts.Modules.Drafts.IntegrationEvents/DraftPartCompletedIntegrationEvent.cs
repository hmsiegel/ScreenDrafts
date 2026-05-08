namespace ScreenDrafts.Modules.Drafts.IntegrationEvents;

public sealed class DraftPartCompletedIntegrationEvent(
  Guid id,
  DateTime occurredOnUtc,
  Guid draftId,
  string draftPublicId,
  string draftPartPublicId,
  int partIndex,
  int totalParts,
  int totalPicks,
  string title,
  string draftType,
  bool isPatreon,
  int? episodeNumber,
  int vetoCount,
  IReadOnlyList<CompletedPickRecord> picks,
  IReadOnlyList<string> participantPublicIds
) : IntegrationEvent(id, occurredOnUtc)
{
  public Guid DraftId { get; set; } = draftId;
  public string DraftPublicId { get; set; } = draftPublicId;
  public string DraftPartPublicId { get; set; } = draftPartPublicId;
  public int PartIndex { get; set; } = partIndex;
  public int TotalParts { get; set; } = totalParts;
  public int TotalPicks { get; set; } = totalPicks;
  public string Title { get; set; } = title;
  public string DraftType { get; set; } = draftType;
  public bool IsPatreon { get; set; } = isPatreon;
  public int? EpisodeNumber { get; set; } = episodeNumber;
  public IReadOnlyList<CompletedPickRecord> Picks { get; set; } = picks;
  public IReadOnlyList<string> ParticipantPublicIds { get; set; } = participantPublicIds;
  public int VetoCount { get; set; } = vetoCount;
}

public sealed record CompletedPickRecord(int Position, string MediaPublicId, string MediaTitle);
