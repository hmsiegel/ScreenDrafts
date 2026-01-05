namespace ScreenDrafts.Modules.Drafts.Domain.Predictions.Entities;

public class SurrogateAssignment : Entity<SurrogateAssignmentId>
{
  private SurrogateAssignment(
    DraftPredictionSet primarySet,
    DraftPredictionSet surrogateSet,
    MergePolicy mergePolicy,
    SurrogateAssignmentId? id = null)
    : base(id ?? SurrogateAssignmentId.CreateUnique())
  {
    PrimarySet = primarySet;
    PrimarySetId = primarySet.Id;
    SurrogateSet = surrogateSet;
    SurrogateSetId = surrogateSet.Id;
    MergePolicy = mergePolicy;
  }

  private SurrogateAssignment()
  {
  }

  public DraftPredictionSet PrimarySet { get; private set; } = default!;    // e.g., Ryan’s set
  public DraftPredictionSetId PrimarySetId { get; private set; } = default!;        // e.g., Ryan’s set

  public DraftPredictionSet SurrogateSet { get; private set; } = default!;  // guest’s set
  public DraftPredictionSetId SurrogateSetId { get; private set; } = default!;      // guest’s set

  public MergePolicy MergePolicy { get; private set; } = default!;  // UseHigherScore, etc.

  public static SurrogateAssignment Create(
    DraftPredictionSet primarySet,
    DraftPredictionSet surrogateSet,
    MergePolicy mergePolicy)
  {
    ArgumentNullException.ThrowIfNull(primarySet);
    ArgumentNullException.ThrowIfNull(surrogateSet);

    return new SurrogateAssignment(
      primarySet: primarySet,
      surrogateSet: surrogateSet,
      mergePolicy: mergePolicy);
  }
}
