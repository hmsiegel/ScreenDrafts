namespace ScreenDrafts.Modules.Drafts.Features.Predictions.SetDraftPartPredictors;

internal sealed class Validator : AbstractValidator<SetDraftPartPredictorsCommand>
{
  public Validator()
  {
    RuleFor(x => x.DraftPartPublicId)
      .NotEmpty()
      .WithMessage("Draft Part Public Id is required.")
      .Must(id => PublicIdGuards.IsValidWithPrefix(id, PublicIdPrefixes.DraftPart))
      .WithMessage("Draft Part Public Id is invalid.");

    RuleFor(x => x.Predictors)
      .NotEmpty()
      .WithMessage("Predictors list cannot be empty.")
      .Must(predictors =>
        predictors.All(p =>
          PublicIdGuards.IsValidWithPrefix(
            p.ContestantPublicId,
            PublicIdPrefixes.PredictionContestant
          )
          && (
            string.IsNullOrEmpty(p.AllowedSubmitterPersonPublicId)
            || PublicIdGuards.IsValidWithPrefix(
              p.AllowedSubmitterPersonPublicId,
              PublicIdPrefixes.Person
            )
          )
        )
      )
      .WithMessage("All Predictor Public Ids must be valid and non-empty.");
  }
}
