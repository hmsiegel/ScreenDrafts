namespace ScreenDrafts.Modules.Drafts.Features.Drafts.SetCategory;

internal sealed class Validator : AbstractValidator<SetCategoryDraftCommand>
{
  public Validator()
  {
    RuleFor(x => x.DraftId)
      .NotEmpty().WithMessage("DraftId is required.");

    RuleFor(x => x.CategoryId)
      .NotEmpty().WithMessage("CategoryId is required.");
  }
}
