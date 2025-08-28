namespace ScreenDrafts.Modules.Drafts.Application.Drafts.Commands.AddCategoryToDraft;

internal sealed class AddCategoryToDraftCommandValidator : AbstractValidator<AddCategoryToDraftCommand>
{
  public AddCategoryToDraftCommandValidator()
  {
    RuleFor(x => x.DraftId).NotEmpty();
    RuleFor(x => x.CategoryId).NotEmpty();
  }
}
