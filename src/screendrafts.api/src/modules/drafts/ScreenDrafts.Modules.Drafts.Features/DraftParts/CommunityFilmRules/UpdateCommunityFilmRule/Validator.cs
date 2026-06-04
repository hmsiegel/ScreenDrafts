namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.CommunityFilmRules.UpdateCommunityFilmRule;

internal sealed class Validator : AbstractValidator<RemoveCommunityFilmRuleCommand>
{
  public Validator()
  {
    RuleFor(x => x.DraftPartId)
      .NotEmpty()
      .WithMessage("Draft part ID is required.")
      .Must(id => PublicIdGuards.IsValidWithPrefix(id, PublicIdPrefixes.DraftPart))
      .WithMessage("Draft part ID must have a valid prefix.");

    RuleFor(x => x.RuleId)
      .NotEmpty()
      .WithMessage("Rule ID is required.")
      .Must(id => PublicIdGuards.IsValidWithPrefix(id, PublicIdPrefixes.CommunityFilmRule))
      .WithMessage("Rule ID must have a valid prefix.");

    RuleFor(x => x.RuleKind)
      .MustBeSmartEnumValue<RemoveCommunityFilmRuleCommand, CommunityFilmRuleKind>()
      .WithMessage("Rule kind must be a valid value.");
    RuleFor(x => x.TargetSlot)
      .GreaterThan(0)
      .When(x => x.TargetSlot.HasValue)
      .WithMessage("Target slot must be greater than 0 when provided.");
  }
}
