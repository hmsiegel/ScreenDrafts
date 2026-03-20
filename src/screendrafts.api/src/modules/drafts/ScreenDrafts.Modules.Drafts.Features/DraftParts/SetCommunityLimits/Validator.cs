namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.SetCommunityLimits;

internal sealed class Validator : AbstractValidator<SetCommunityLimitsCommand>
{
  public Validator()
  {
    RuleFor(x => x.DraftPartId)
      .NotEmpty()
      .WithMessage("Draft part ID is required.")
      .Must(id => PublicIdGuards.IsValidWithPrefix(id, PublicIdPrefixes.DraftPart))
      .WithMessage("Draft part ID must be a valid public ID with the correct prefix.");
    RuleFor(x => x.MaxCommunityPicks)
      .GreaterThanOrEqualTo(0)
      .WithMessage("Max community picks must be greater than or equal to 0.");
    RuleFor(x => x.MaxCommunityVetoes)
      .GreaterThanOrEqualTo(0)
      .WithMessage("Max community vetoes must be greater than or equal to 0.");
  }
}
