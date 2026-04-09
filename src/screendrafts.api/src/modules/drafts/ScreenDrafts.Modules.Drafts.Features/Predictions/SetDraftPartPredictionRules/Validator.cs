namespace ScreenDrafts.Modules.Drafts.Features.Predictions.SetDraftPartPredictionRules;

// ============================================================
// Validator
// ============================================================

internal sealed class Validator
  : AbstractValidator<SetDraftPartPredictionRulesCommand>
{
  public Validator()
  {
    RuleFor(x => x.DraftPartPublicId)
      .NotEmpty()
      .WithMessage("DraftPartPublicId is required.")
      .Must(id => PublicIdGuards.IsValidWithPrefix(id, PublicIdPrefixes.DraftPart))
      .WithMessage("DraftPartPublicId must be a valid public ID with the correct prefix.");

    RuleFor(x => x.PredictionMode)
      .MustBeSmartEnumValue<SetDraftPartPredictionRulesCommand, PredictionMode>()
      .WithMessage("PredictionMode must be a valid value.");

    RuleFor(x => x.RequiredCount)
      .GreaterThan(0)
      .WithMessage("RequiredCount must be greater than 0.")
      .LessThanOrEqualTo(50)
      .WithMessage("RequiredCount must be less than or equal to 50.");

    RuleFor(x => x.TopN)
      .GreaterThan(0)
      .When(x => x.TopN.HasValue)
      .WithMessage("TopN must be greater than 0 when specified.");
  }
}
