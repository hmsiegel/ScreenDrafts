namespace ScreenDrafts.Modules.Drafts.Application.Drafts.Commands.RemoveCategory;

internal sealed class RemoveCategoryCommandValidator : AbstractValidator<RemoveCategoryCommand>
{
  public RemoveCategoryCommandValidator()
  {
    RuleFor(x => x.CategoryId)
      .NotNull()
      .WithMessage("CategoryId is required.");
  }
}
