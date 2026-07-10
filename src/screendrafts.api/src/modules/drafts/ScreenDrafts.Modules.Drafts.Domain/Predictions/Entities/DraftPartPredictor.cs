namespace ScreenDrafts.Modules.Drafts.Domain.Predictions.Entities;

/// <summary>
/// One predictor slot on a draft part. Contestant is who gets scored.
/// AllowedSubmitterPersonId, when set, is who may submit on the
/// contestant's behalf (a stand-in host, not the contestant themself).
/// A blank slot (no override) restricts submission to the contestant's
/// own linked person.
/// </summary>
public sealed class DraftPartPredictor : Entity<DraftPartPredictorId>
{
  private DraftPartPredictor(
    string publicId,
    DraftPartId draftPartId,
    PredictionContestant contestant,
    PersonId? allowedSubmitterPersonId,
    DraftPartPredictorId? id = null
  )
    : base(id ?? DraftPartPredictorId.CreateUnique())
  {
    PublicId = publicId;
    DraftPartId = draftPartId;
    Contestant = contestant;
    ContestantId = contestant.Id;
    AllowedSubmitterPersonId = allowedSubmitterPersonId;
  }

  private DraftPartPredictor() { }

  public string PublicId { get; private set; } = default!;
  public DraftPartId DraftPartId { get; private set; } = default!;
  public PredictionContestant Contestant { get; private set; } = default!;
  public ContestantId ContestantId { get; private set; } = default!;
  public PersonId? AllowedSubmitterPersonId { get; private set; }

  public static DraftPartPredictor Create(
    string publicId,
    DraftPartId draftPartId,
    PredictionContestant contestant,
    PersonId? allowedSubmitterPersonId = null
  )
  {
    ArgumentNullException.ThrowIfNull(contestant);

    return new DraftPartPredictor(
      publicId: publicId,
      draftPartId: draftPartId,
      contestant: contestant,
      allowedSubmitterPersonId: allowedSubmitterPersonId
    );
  }

  public void SetAllowedSubmitter(PersonId? personId) => AllowedSubmitterPersonId = personId;
}
