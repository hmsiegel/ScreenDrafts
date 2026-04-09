namespace ScreenDrafts.Modules.Drafts.Domain.Predictions.Entities;

public sealed class DraftPartPredictionRule : Entity<DraftPartPredictionRuleId>
{
  private DraftPartPredictionRule(
    string publicId,
    DraftPart draftPart,
    PredictionMode predictionMode,
    int requiredCount,
    int? topN,
    DateTime? deadlineUtc,
    DraftPartPredictionRuleId? id = null)
    : base(id ?? DraftPartPredictionRuleId.CreateUnique())
  {
    PredictionMode = predictionMode;
    RequiredCount = requiredCount;
    TopN = topN;
    DeadlineUtc = deadlineUtc;
    PublicId = publicId;
    DraftPart = draftPart;
    DraftPartId = draftPart.Id;
  }
  private DraftPartPredictionRule()
  {
  }

  public string PublicId { get; private set; } = default!;
  public DraftPart DraftPart { get; private set; } = default!;
  public DraftPartId DraftPartId { get; private set; } = default!;

  public PredictionMode PredictionMode { get; private set; } = PredictionMode.UnorderedAll;
  public int RequiredCount { get; private set; } = 7;

  /// <summary>
  /// Null when mode is UnorderedAll or OrderedAll (every pick counts).
  /// Required when mode is UnorderedTopN or OrderedTopN (only top N picks count).
  /// </summary>
  public int? TopN { get; private set; }

  /// <summary>
  /// Submissionr are rejected after this deadline. Null if no deadline.
  /// (close manually via DraftPart status transition)
  /// </summary>
  public DateTime? DeadlineUtc { get; private set; } 

  public DateTime CreatedOnUtc { get; private set; } = DateTime.UtcNow;
  public DateTime? UpdatedOnUtc { get; private set; } = default!;

  /// <param name="topN">Required when mode is UnorderedTopN or OrderedTopN.</param>
  /// <param name="deadlineUtc">If set, submissions are rejected after this time.</param>
  /// <returns></returns>
  public static Result<DraftPartPredictionRule> Create(
    string publicId,
    DraftPart draftPart,
    PredictionMode predictionMode,
    int requiredCount = 7,
    int? topN = null,
    DateTime? deadlineUtc = null)
  {
    ArgumentNullException.ThrowIfNull(draftPart);
    ArgumentNullException.ThrowIfNull(predictionMode);

    var isTopNMode =
      predictionMode == PredictionMode.UnorderedTopN
      || predictionMode == PredictionMode.OrderedTopN;

    if (isTopNMode && topN is null)
    {
      return Result.Failure<DraftPartPredictionRule>(
        PredictionErrors.TopNRequiredForPredictionMode(predictionMode.Name));
    }

    if (!isTopNMode && topN is not null)
    {
      return Result.Failure<DraftPartPredictionRule>(
        PredictionErrors.TopNNotAllowedForPredictionMode(predictionMode.Name));
    }

    return new DraftPartPredictionRule(
      publicId: publicId,
      draftPart: draftPart,
      predictionMode: predictionMode,
      requiredCount: requiredCount,
      topN: topN,
      deadlineUtc: deadlineUtc);
  }

  /// <summary>
  /// Produces a frozen snapshot of the current rules for attaching to a locked
  /// <see cref="DraftPredictionSet"/>. Scoring always uses the live rules. This 
  /// snapshot is for audit and display only.
  /// </summary>
  /// <returns></returns>
  public PredictionRulesSnapshot ToSnapshot()
  {
    return new PredictionRulesSnapshot(
      Mode: PredictionMode,
      RequiredCount: RequiredCount,
      TopN: TopN);
  }
}
