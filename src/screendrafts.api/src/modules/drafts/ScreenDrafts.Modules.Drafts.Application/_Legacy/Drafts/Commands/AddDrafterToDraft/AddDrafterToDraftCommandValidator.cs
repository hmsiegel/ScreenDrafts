namespace ScreenDrafts.Modules.Drafts.Application._Legacy.Drafts.Commands.AddDrafterToDraft;

internal sealed class AddDrafterToDraftCommandValidator : AbstractValidator<AddDrafterToDraftCommand>
{
  public AddDrafterToDraftCommandValidator()
  {
    RuleFor(x => x.DrafterId).NotEmpty();
    RuleFor(x => x.DraftPartId).NotEmpty();
  }
}
