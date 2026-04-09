namespace ScreenDrafts.Modules.Drafts.Features.Predictions.ScoreDraftPartPredictions;

internal sealed class Validator : AbstractValidator<ScoreDraftPartPredictionsCommand>
{
  public Validator()
  {
    RuleFor(x => x.DraftPartPublicId)
      .NotEmpty()
      .WithMessage("Draft part public ID is required.")
      .Must(id => PublicIdGuards.IsValidWithPrefix(id, PublicIdPrefixes.DraftPart))
      .WithMessage("Draft part public ID must start with the correct prefix.");

    RuleFor(x => x.FinalMediaPublicIds).NotEmpty();

    RuleForEach(x => x.FinalMediaPublicIds)
      .NotEmpty()
      .WithMessage("Final media public IDs cannot contain empty values.")
      .Must(id => PublicIdGuards.IsValidWithPrefix(id, PublicIdPrefixes.Media))
      .WithMessage("Each final media public ID must start with the correct prefix.");
  }
}
