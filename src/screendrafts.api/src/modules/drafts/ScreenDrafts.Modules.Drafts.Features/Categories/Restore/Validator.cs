namespace ScreenDrafts.Modules.Drafts.Features.Categories.Restore;

internal sealed class Validator : AbstractValidator<RestoreCategoryCommand>
{
  public Validator()
  {
    RuleFor(x => x.PublicId)
      .NotEmpty()
      .Must(publicId => PublicIdGuards.IsValidWithPrefix(publicId, PublicIdPrefixes.Category))
      .WithMessage("PublicId is required.");
  }
}

