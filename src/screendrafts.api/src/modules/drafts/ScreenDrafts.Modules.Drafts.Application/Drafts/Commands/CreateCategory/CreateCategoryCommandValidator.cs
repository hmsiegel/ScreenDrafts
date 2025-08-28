namespace ScreenDrafts.Modules.Drafts.Application.Drafts.Commands.AddCategory;

internal sealed class CreateCategoryCommandValidator : AbstractValidator<CreateCategoryCommand>
{
  public CreateCategoryCommandValidator()
  {
    RuleFor(x => x.Name)
      .NotEmpty().WithMessage(CategoryErrors.CategoryNameIsRequired.Description)
      .MaximumLength(Category.NameMaxLength)
      .WithMessage(CategoryErrors.CategoryNameTooLong(Category.NameMaxLength).Description);

    RuleFor(x => x.Description)
      .MaximumLength(Category.DescriptionMaxLength)
      .WithMessage(CategoryErrors.CategoryDescriptionTooLong(Category.DescriptionMaxLength).Description);
  }
}
