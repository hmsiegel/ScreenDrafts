using ScreenDrafts.Modules.Drafts.Domain.Predictions.Enums;
using ScreenDrafts.Modules.Drafts.Domain.Predictions.ValueObjects;

namespace ScreenDrafts.Modules.Drafts.Domain.Predictions.Entities;

public sealed class DraftPartPredictionRules : Entity<DraftPartPredictionRulesId>
{
  private DraftPartPredictionRules(
    DraftPart draftPart,
    PredictionMode predictionMode,
    DraftPartPredictionRulesId? id = null,
    int requiredCount = 7)
    : base(id ?? DraftPartPredictionRulesId.CreateUnique())
  {
    PredictionMode = predictionMode;
    RequiredCount = requiredCount;
    DraftPart = draftPart;
    DraftPartId = draftPart.Id;
  }
  private DraftPartPredictionRules()
  {
  }

  public DraftPart DraftPart { get; private set; } = default!;
  public DraftPartId DraftPartId { get; private set; } = default!;

  public PredictionMode PredictionMode { get; private set; } = PredictionMode.UnorderedAll;
  public int RequiredCount { get; private set; } = 7;
  public int? TopN { get; private set; } = 0;
  public bool IsOrderRequired => 
    PredictionMode == PredictionMode.OrderedAll || PredictionMode == PredictionMode.OrderedTopN;
  public int? MultiplierForPerfectOrder => 
    IsOrderRequired ? 2 : null;

  public DateTime CreatedOnUtc { get; private set; } = DateTime.UtcNow;
  public DateTime? UpdatedOnUtc { get; private set; } = default!;

  public static DraftPartPredictionRules Create(
    DraftPart draftPart,
    PredictionMode predictionMode,
    int requiredCount = 7)
  {
    ArgumentNullException.ThrowIfNull(draftPart);

    return new DraftPartPredictionRules(
      draftPart: draftPart,
      predictionMode: predictionMode,
      requiredCount: requiredCount);
  }
}
