namespace ScreenDrafts.Modules.Drafts.Features.DrafterTeams.AddDrafterToTeam;

internal sealed class Validator : AbstractValidator<AddDrafterToTeamCommand>
{
  public Validator()
  {
    RuleFor(x => x.DrafterTeamId)
      .NotEmpty()
      .WithMessage("Drafter team ID is required.")
      .Must(id => PublicIdGuards.IsValidWithPrefix(id, PublicIdPrefixes.DrafterTeam))
      .WithMessage("Invalid drafter team ID.");

    RuleFor(x => x.DrafterId)
      .NotEmpty()
      .WithMessage("Drafter ID is required.")
      .Must(id => PublicIdGuards.IsValidWithPrefix(id, PublicIdPrefixes.Drafter))
      .WithMessage("Invalid drafter ID.");
  }
}
