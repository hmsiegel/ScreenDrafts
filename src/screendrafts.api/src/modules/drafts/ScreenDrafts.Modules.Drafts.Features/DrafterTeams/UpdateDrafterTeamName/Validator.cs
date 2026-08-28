namespace ScreenDrafts.Modules.Drafts.Features.DrafterTeams.UpdateDrafterTeamName;

internal sealed class Validator : AbstractValidator<UpdateDrafterTeamNameRequest>
{
  public Validator()
  {
    RuleFor(x => x.DrafterTeamId)
      .NotEmpty()
      .WithMessage("DrafterTeamId is required.")
      .Must(id => PublicIdGuards.IsValidWithPrefix(id, PublicIdPrefixes.DrafterTeam))
      .WithMessage("DrafterTeamId must be a valid public ID with the correct prefix.");

    RuleFor(x => x.Name).NotEmpty().WithMessage("DrafterTeamName is required.");
  }
}
