namespace ScreenDrafts.Modules.Drafts.Application.Drafts.Commands.AddDrafterTeamToDraft;

internal sealed class AddDrafterTeamToDraftCommandValidator : AbstractValidator<AddDrafterTeamToDraftCommand>
{
  public AddDrafterTeamToDraftCommandValidator()
  {
    RuleFor(x => x.DrafterTeamId).NotEmpty().WithMessage("Drafter Team Id is required.");
    RuleFor(x => x.DraftPartId).NotEmpty().WithMessage("Draft Part Id is required.");
  }
}
