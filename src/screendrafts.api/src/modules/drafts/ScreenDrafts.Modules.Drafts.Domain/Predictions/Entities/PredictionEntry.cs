namespace ScreenDrafts.Modules.Drafts.Domain.Predictions.Entities;

public sealed class PredictionEntry : Entity<PredictionEntryId>
{
  private PredictionEntry(
    DraftPredictionSet predictionSet,
    int tmdbId,
    string mediaTitle,
    int? orderIndex = null,
    string? notes = null,
    bool? isCorrect = null,
    string? mediaPublicId = null,
    PredictionEntryId? id = null
  )
    : base(id ?? PredictionEntryId.CreateUnique())
  {
    PredictionSet = predictionSet;
    SetId = predictionSet.Id;
    TmdbId = tmdbId;
    IsCorrect = isCorrect;
    MediaPublicId = mediaPublicId;
    MediaTitle = mediaTitle;

    OrderIndex = orderIndex;
    Notes = notes;
  }

  private PredictionEntry() { }

  public DraftPredictionSet PredictionSet { get; private set; } = default!;
  public DraftPredictionSetId SetId { get; private set; } = default!;

  /// <summary>
  /// TMDb id of the predicted title — the same identifier the draft board,
  /// candidate list, and community film rules key off. The movie need not
  /// exist in the Movies database at submission time; FetchMediaRequestedIntegrationEvent
  /// syncs it, same as AddMovieToDraftBoard.
  /// </summary>
  public int TmdbId { get; private set; }

  public string? MediaPublicId { get; private set; }

  /// <summary>
  /// Title captured at submission time. Stored so the entry is human-readable
  /// even if the Movies record is later corrected.
  /// </summary>
  public string MediaTitle { get; private set; } = default!;

  /// <summary>
  /// Null when mode is unordered, otherwise the 1-based rank of this entry in the prediction set. Stored
  /// at submission time to ensure consistency even if the prediction rules change later.
  /// </summary>
  public int? OrderIndex { get; private set; }
  public string? Notes { get; private set; }
  public bool? IsCorrect { get; private set; }

  public static PredictionEntry Create(
    DraftPredictionSet predictionSet,
    int tmdbId,
    string mediaTitle,
    int? orderIndex = null,
    string? notes = null,
    bool? isCorrect = null
  )
  {
    ArgumentNullException.ThrowIfNull(predictionSet);
    ArgumentNullException.ThrowIfNull(mediaTitle);

    return new PredictionEntry(
      predictionSet: predictionSet,
      tmdbId: tmdbId,
      mediaTitle: mediaTitle,
      orderIndex: orderIndex,
      notes: notes,
      isCorrect: isCorrect
    );
  }

  public void MarkCorrect(bool isCorrect) => IsCorrect = isCorrect;
}
