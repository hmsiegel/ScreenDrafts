namespace ScreenDrafts.Modules.Drafts.Features.Categories.Edit;

internal sealed class Validator : AbstractValidator<EditCategoryCommand>
{
  public Validator()
  {
    RuleFor(x => x.PublicId)
      .NotEmpty()
      .Must(publicId => PublicIdGuards.IsValidWithPrefix(publicId, PublicIdPrefixes.Category))
      .WithMessage("Campaign ID must be provided.");

    RuleFor(x => x.Name)
      .MaximumLength(100).WithMessage("Campaign name must not exceed 100 characters.")
      .When(x => x.Name is not null);

    RuleFor(x => x.Description)
      .MaximumLength(500).WithMessage("Category description must not exceed 500 characters.")
      .When(x => x.Description is not null);
  }
}

