namespace ScreenDrafts.Modules.Drafts.Features.Predictions.ClosePredictionSeason;

internal sealed class Validator : AbstractValidator<ClosePredictionSeasonCommand>
{
  public Validator()
  {
    RuleFor(x => x.SeasonPublicId)
      .NotEmpty()
      .WithMessage("Season public ID is required.")
      .Must(id => PublicIdGuards.IsValidWithPrefix(id, PublicIdPrefixes.PredictionSeason))
      .WithMessage("Season public ID is invalid.");

    RuleFor(x => x.EndsOn).NotEmpty();
  }
}
