namespace ScreenDrafts.Modules.Drafts.Features.Predictions.LockPredictionSet;

internal sealed class Validator : AbstractValidator<LockPredictionSetCommand>
{
  public Validator()
  {
    RuleFor(x => x.DraftPartPublicId)
      .NotEmpty()
      .WithMessage("Draft part public ID is required.")
      .Must(x => PublicIdGuards.IsValidWithPrefix(x, PublicIdPrefixes.DraftPart))
      .WithMessage("Draft part public ID is invalid.");

    RuleFor(x => x.SetPublicId)
      .NotEmpty()
      .WithMessage("Set public ID is required.")
      .Must(x => PublicIdGuards.IsValidWithPrefix(x, PublicIdPrefixes.DraftPredictionSet))
      .WithMessage("Set public ID is invalid.");
  }
}
