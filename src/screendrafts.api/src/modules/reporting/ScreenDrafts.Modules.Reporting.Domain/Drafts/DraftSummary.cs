namespace ScreenDrafts.Modules.Reporting.Domain.Drafts;

public sealed class DraftSummary : Entity<DraftSummaryId>
{
  private DraftSummary(
    Guid draftId,
    string draftPublicId,
    string draftPartPublicId,
    string title,
    string draftType,
    int partIndex,
    int totalParts,
    bool isPatreon,
    int? episodeNumber,
    bool isComplete,
    DateTime? completedAtUtc,
    DateTime createdAtUtc,
    DraftSummaryId? id = null
  )
    : base(id ?? DraftSummaryId.CreateUnique())
  {
    DraftId = draftId;
    DraftPublicId = draftPublicId;
    DraftPartPublicId = draftPartPublicId;
    Title = title;
    DraftType = draftType;
    PartIndex = partIndex;
    TotalParts = totalParts;
    IsPatreon = isPatreon;
    EpisodeNumber = episodeNumber;
    IsComplete = isComplete;
    CompletedAtUtc = completedAtUtc;
    CreatedAtUtc = createdAtUtc;
  }

  private DraftSummary() { }

  public Guid DraftId { get; private set; }
  public string DraftPublicId { get; private set; } = default!;
  public string DraftPartPublicId { get; private set; } = default!;
  public string Title { get; private set; } = default!;
  public string DraftType { get; private set; } = default!;
  public int PartIndex { get; private set; }
  public int TotalParts { get; private set; }
  public bool IsPatreon { get; private set; }
  public int? EpisodeNumber { get; private set; }
  public bool IsComplete { get; private set; }
  public DateTime? CompletedAtUtc { get; private set; }
  public DateTime CreatedAtUtc { get; private set; }

  public static DraftSummary Create(
    Guid draftId,
    string draftPublicId,
    string draftPartPublicId,
    string title,
    string draftType,
    int partIndex,
    int totalParts,
    bool isPatreon,
    int? episodeNumber,
    bool isComplete,
    DateTime? completedAtUtc,
    DateTime createdAtUtc
  )
  {
    return new DraftSummary(
      draftId,
      draftPublicId,
      draftPartPublicId,
      title,
      draftType,
      partIndex,
      totalParts,
      isPatreon,
      episodeNumber,
      isComplete,
      completedAtUtc,
      createdAtUtc
    );
  }

  public void Update(int totalParts, int? episodeNumber, bool isPatreon)
  {
    TotalParts = totalParts;
    IsPatreon = isPatreon;

    if (episodeNumber.HasValue)
    {
      EpisodeNumber = episodeNumber;
    }
  }

  public void MarkComplete()
  {
    IsComplete = true;
    CompletedAtUtc = DateTime.UtcNow;
  }

  public void SetEpisodeNumber(int episodeNumber)
  {
    EpisodeNumber = episodeNumber;
  }
}
