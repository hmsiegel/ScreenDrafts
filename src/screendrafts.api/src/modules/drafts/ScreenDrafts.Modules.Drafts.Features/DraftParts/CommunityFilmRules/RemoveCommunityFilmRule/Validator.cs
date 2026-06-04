namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.CommunityFilmRules.RemoveCommunityFilmRule;

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
  }
}
