namespace ScreenDrafts.Modules.Drafts.Domain.Predictions.Entities;

public sealed class PredictionEntry : Entity<PredictionEntryId>
{
  private PredictionEntry(
    DraftPredictionSet predictionSet,
    string mediaPublicId,
    string mediaTitle,
    int? orderIndex = null,
    string? notes = null,
    PredictionEntryId? id = null)
    : base(id ?? PredictionEntryId.CreateUnique())
  {
    PredictionSet = predictionSet;
    SetId = predictionSet.Id;

    MediaPublicId = mediaPublicId;
    MediaTitle = mediaTitle;

    OrderIndex = orderIndex;
    Notes = notes;
  }

  private PredictionEntry()
  {
  }

  public DraftPredictionSet PredictionSet { get; private set; } = default!;
  public DraftPredictionSetId SetId { get; private set; } = default!;

  /// <summary>
  /// Public Id from the Movies module.
  /// </summary>
  public string MediaPublicId { get; private set; } = default!; 

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

  public static PredictionEntry Create(
    DraftPredictionSet predictionSet,
    string mediaPublicId,
    string mediaTitle,
    int? orderIndex = null,
    string? notes = null)
  {
    ArgumentNullException.ThrowIfNull(predictionSet);
    ArgumentNullException.ThrowIfNull(mediaPublicId);
    ArgumentNullException.ThrowIfNull(mediaTitle);

    return new PredictionEntry(
      predictionSet: predictionSet,
      mediaPublicId: mediaPublicId,
      mediaTitle: mediaTitle,
      orderIndex: orderIndex,
      notes: notes);
  }
}
