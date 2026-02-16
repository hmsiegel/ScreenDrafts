namespace ScreenDrafts.Modules.Drafts.Application._Legacy.Drafts.Commands.RemoveCategoryFromDraft;

internal sealed class RemoveCategoryFromDraftCommandValidator : AbstractValidator<RemoveCategoryFromDraftCommand>
{
  public RemoveCategoryFromDraftCommandValidator()
  {
    RuleFor(x => x.DraftId).NotEmpty().WithMessage("DraftId is required.");
    RuleFor(x => x.CategoryId).NotEmpty().WithMessage("CategoryId is required.");
  }
}
