namespace ScreenDrafts.Modules.Drafts.Features.DrafterTeams.RemoveDrafterFromTeam;

internal sealed class Validator : AbstractValidator<RemoveDrafterFromTeamCommand>
{
  public Validator()
  {
    RuleFor(x => x.DrafterTeamId)
      .NotEmpty()
      .WithMessage("Drafter team ID is required.")
      .Must(id => PublicIdGuards.IsValidWithPrefix(id, PublicIdPrefixes.DrafterTeam))
      .WithMessage("Drafter team ID must be a valid public ID with the correct prefix.");

    RuleFor(x => x.DrafterId)
      .NotEmpty()
      .WithMessage("Drafter ID is required.")
      .Must(id => PublicIdGuards.IsValidWithPrefix(id, PublicIdPrefixes.Drafter))
      .WithMessage("Drafter ID must be a valid public ID with the correct prefix.");
  }
}
