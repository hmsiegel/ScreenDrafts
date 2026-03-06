namespace ScreenDrafts.Modules.Drafts.Features.Drafts.RemoveCategoryFromDraft;

internal sealed class RemoveCategoryFromDraftValidator : AbstractValidator<RemoveCategoryFromDraftCommand>
{
  public RemoveCategoryFromDraftValidator()
  {
    RuleFor(x => x.DraftId)
       .NotEmpty().WithMessage("DraftId is required.")
       .Must(x => PublicIdGuards.IsValidWithPrefix(x, PublicIdPrefixes.Draft))
       .WithMessage("DraftId is invalid.");
    RuleFor(x => x.CategoryId)
       .NotEmpty().WithMessage("CategoryId is required.")
       .Must(x => PublicIdGuards.IsValidWithPrefix(x, PublicIdPrefixes.Category))
       .WithMessage("CategoryId is invalid.");
  }
}
