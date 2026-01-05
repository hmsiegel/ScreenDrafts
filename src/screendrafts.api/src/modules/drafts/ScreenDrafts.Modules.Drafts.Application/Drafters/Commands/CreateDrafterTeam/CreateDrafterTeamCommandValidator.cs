namespace ScreenDrafts.Modules.Drafts.Application.Drafters.Commands.CreateDrafterTeam;

internal sealed class CreateDrafterTeamCommandValidator : AbstractValidator<CreateDrafterTeamCommand>
{
  public CreateDrafterTeamCommandValidator()
  {
    RuleFor(x => x.TeamName)
      .NotEmpty()
      .MaximumLength(DrafterTeam.TeamNameMaxLength);
  }
}
