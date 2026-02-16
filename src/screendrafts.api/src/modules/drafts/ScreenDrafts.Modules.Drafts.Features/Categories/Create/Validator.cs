namespace ScreenDrafts.Modules.Drafts.Features.Categories.Create;

internal sealed class Validator : AbstractValidator<CreateCategoryCommand>
{
  public Validator()
  {
    RuleFor(x => x.Name)
      .NotEmpty()
      .MaximumLength(100);
    RuleFor(x => x.Description)
      .NotEmpty()
      .MaximumLength(500);
  }
}

