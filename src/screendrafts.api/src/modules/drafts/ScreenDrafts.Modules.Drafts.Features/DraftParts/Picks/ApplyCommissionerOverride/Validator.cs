namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.Picks.ApplyCommissionerOverride;

internal sealed class Validator : AbstractValidator<ApplyCommissionerOverrideCommand>
{
  public Validator()
  {
    RuleFor(x => x.DraftPartId)
      .NotEmpty()
      .WithMessage("DraftPartId is required.")
      .Must(id => PublicIdGuards.IsValidWithPrefix(id, PublicIdPrefixes.DraftPart))
      .WithMessage("Invalid DraftPartId.");

    RuleFor(x => x.PlayOrder)
      .GreaterThan(0)
      .WithMessage("PlayOrder must be greater than 0.");
  }
}


