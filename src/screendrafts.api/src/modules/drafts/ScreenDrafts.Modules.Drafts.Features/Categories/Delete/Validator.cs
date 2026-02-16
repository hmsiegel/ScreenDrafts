namespace ScreenDrafts.Modules.Drafts.Features.Categories.Delete;

internal sealed class Validator : AbstractValidator<DeleteCategoryCommand>
{
  public Validator()
  {
    RuleFor(x => x.PublicId)
      .NotEmpty()
      .Must(publicId => PublicIdGuards.IsValidWithPrefix(publicId, PublicIdPrefixes.Category))
      .WithMessage("PublicId is required.");
  }
}

