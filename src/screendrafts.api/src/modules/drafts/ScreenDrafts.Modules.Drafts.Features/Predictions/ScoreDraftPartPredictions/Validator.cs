namespace ScreenDrafts.Modules.Drafts.Features.Predictions.ScoreDraftPartPredictions;

internal sealed class Validator : AbstractValidator<ScoreDraftPartPredictionsCommand>
{
  public Validator()
  {
    RuleFor(x => x.DraftPartId)
      .NotEmpty()
      .WithMessage("Draft part public ID is required.")
      .Must(id => PublicIdGuards.IsValidWithPrefix(id, PublicIdPrefixes.DraftPart))
      .WithMessage("Draft part public ID must start with the correct prefix.");

    RuleFor(x => x.FinalTmdbIds).NotEmpty();
  }
}
