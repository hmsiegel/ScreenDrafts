namespace ScreenDrafts.Modules.Drafts.Features.Predictions.AddCarryover;

internal sealed class AddCarryoverCommandValidator : AbstractValidator<AddCarryoverCommand>
{
  public AddCarryoverCommandValidator()
  {
    RuleFor(x => x.SeasonPublicId)
      .NotEmpty()
      .WithMessage("SeasonPublicId is required.")
      .Must(id => PublicIdGuards.IsValidWithPrefix(id,PublicIdPrefixes.PredictionSeason))
      .WithMessage("SeasonPublicId must be a valid season ID.");

    RuleFor(x => x.ContestantPublicId)
      .NotEmpty()
      .WithMessage("ContestantPublicId is required.")
      .Must(id => PublicIdGuards.IsValidWithPrefix(id, PublicIdPrefixes.PredictionContestant))
      .WithMessage("ContestantPublicId must be a valid contestant ID.");

    RuleFor(x => x.Points)
      .NotEqual(0)
      .WithMessage("Carryover points must be non-zero.");

    RuleFor(x => x.Kind)
      .MustBeSmartEnumValue<AddCarryoverCommand, CarryoverKind>()
      .WithMessage("Kind must be a valid CarryoverKind value.");

    RuleFor(x => x.Reason).MaximumLength(500);
  }
}
