namespace ScreenDrafts.Modules.Drafts.Application.Drafts.Commands.RemoveDrafterFromDraft;

internal sealed class RemoveDrafterFromDraftCommandValidator : AbstractValidator<RemoveDrafterFromDraftCommand>
{
  public RemoveDrafterFromDraftCommandValidator()
  {
    RuleFor(x => x.DraftId).NotEmpty();
    RuleFor(x => x.DrafterId).NotEmpty();
  }
}
