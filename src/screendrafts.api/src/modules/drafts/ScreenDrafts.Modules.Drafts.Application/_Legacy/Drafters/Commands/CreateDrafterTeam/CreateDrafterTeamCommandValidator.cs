namespace ScreenDrafts.Modules.Drafts.Application._Legacy.Drafters.Commands.CreateDrafterTeam;

internal sealed class CreateDrafterTeamCommandValidator : AbstractValidator<CreateDrafterTeamCommand>
{
  public CreateDrafterTeamCommandValidator()
  {
    RuleFor(x => x.TeamName)
      .NotEmpty()
      .MaximumLength(DrafterTeam.TeamNameMaxLength);
  }
}
