namespace ScreenDrafts.Modules.Drafts.Application.Drafters.Commands.AddDrafterToDrafterTeam;

internal sealed class AddDrafterToDrafterTeamCommandValidator : AbstractValidator<AddDrafterToDrafterTeamCommand>
{
  public AddDrafterToDrafterTeamCommandValidator()
  {
    RuleFor(x => x.DrafterId).NotEmpty();
    RuleFor(x => x.DrafterTeamId).NotEmpty();
  }
}
