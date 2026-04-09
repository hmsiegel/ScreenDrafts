namespace ScreenDrafts.Modules.Drafts.Features.Predictions.CreatePredictionSeason;

internal sealed class Validator : AbstractValidator<CreatePredictionSeasonRequest>
{
  public Validator()
  {
    RuleFor(x => x.Number)
      .GreaterThan(0)
      .WithMessage("Season number must be greater than 0.");

    RuleFor(x => x.StartsOn)
      .NotEmpty()
      .WithMessage("Start date is required.");
  }
}
