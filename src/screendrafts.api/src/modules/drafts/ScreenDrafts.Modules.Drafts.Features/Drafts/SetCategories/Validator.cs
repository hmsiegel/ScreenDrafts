namespace ScreenDrafts.Modules.Drafts.Features.Drafts.SetCategories;

internal sealed class Validator : AbstractValidator<SetCategoriesDraftRequest>
{
  public Validator()
  {
    RuleFor(x => x.DraftId)
      .NotEmpty()
      .WithMessage("Draft ID is required.");
    RuleFor(x => x.CategoryIds)
      .NotNull()
      .WithMessage("Category IDs are required.")
      .Must(ids => ids.All(id => !string.IsNullOrWhiteSpace(id)))
      .WithMessage("Category IDs cannot contain empty or whitespace values.");
  }
}
