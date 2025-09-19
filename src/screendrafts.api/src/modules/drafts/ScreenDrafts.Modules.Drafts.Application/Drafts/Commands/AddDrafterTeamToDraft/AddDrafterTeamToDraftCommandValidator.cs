namespace ScreenDrafts.Modules.Drafts.Application.Drafts.Commands.AddDrafterTeamToDraft;

internal sealed class AddDrafterTeamToDraftCommandValidator : AbstractValidator<AddDrafterTeamToDraftCommand>
{
  public AddDrafterTeamToDraftCommandValidator()
  {
    RuleFor(x => x.DraftId).NotEmpty().WithMessage(DraftErrors.DraftIsRequired.ToString());
    RuleFor(x => x.DrafterTeamId).NotEmpty().WithMessage("Drafter Team Id is required.");
  }
}
