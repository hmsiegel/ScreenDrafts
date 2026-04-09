namespace ScreenDrafts.Modules.Drafts.Features.Predictions.AssignSurrogate;

internal sealed class Validator : AbstractValidator<AssignSurrogateCommand>
{
  public Validator()
  {
    RuleFor(x => x.DraftPartPublicId)
      .NotEmpty()
      .WithMessage("DraftPartPublicId is required.")
      .Must(id => PublicIdGuards.IsValidWithPrefix(id, PublicIdPrefixes.DraftPart))
      .WithMessage("DraftPartPublicId must be a valid public ID with the correct prefix.");

    RuleFor(x => x.PrimarySetPublicId)
      .NotEmpty()
      .WithMessage("PrimarySetPublicId is required.")
      .Must(id => PublicIdGuards.IsValidWithPrefix(id, PublicIdPrefixes.DraftPredictionSet))
      .WithMessage("PrimarySetPublicId must be a valid public ID with the correct prefix.");

    RuleFor(x => x.SurrogateSetPublicId)
      .NotEmpty()
      .WithMessage("SurrogateSetPublicId is required.")
      .Must(id => PublicIdGuards.IsValidWithPrefix(id, PublicIdPrefixes.DraftPredictionSet))
      .WithMessage("SurrogateSetPublicId must be a valid public ID with the correct prefix.");

    RuleFor(x => x.MergePolicy)
      .MustBeSmartEnumValue<AssignSurrogateCommand, MergePolicy>()
      .WithMessage("MergePolicy must be a valid value.");

    RuleFor(x => x)
      .Must(x => x.PrimarySetPublicId != x.SurrogateSetPublicId)
      .WithMessage("Primary and surrogate sets must be different.");
  }
}
