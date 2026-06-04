namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.CommunityFilmRules.AssignFilmToCommunityFilmRule;

internal sealed class Validator : AbstractValidator<AssignFilmToCommunityFilmRuleCommand>
{
  public Validator()
  {
    RuleFor(x => x.DraftPartId)
      .NotEmpty()
      .WithMessage("DraftPartId is required.")
      .Must(id => PublicIdGuards.IsValidWithPrefix(id, PublicIdPrefixes.DraftPart))
      .WithMessage("DraftPartId is invalid.");

    RuleFor(x => x.RuleId)
      .NotEmpty()
      .WithMessage("RuleId is required.")
      .Must(id => PublicIdGuards.IsValidWithPrefix(id, PublicIdPrefixes.CommunityFilmRule))
      .WithMessage("RuleId is invalid.");

    RuleFor(x => x.TmdbId)
      .NotEmpty()
      .WithMessage("TmdbId is required.")
      .GreaterThan(0)
      .WithMessage("TmdbId must be greater than 0.");
  }
}
