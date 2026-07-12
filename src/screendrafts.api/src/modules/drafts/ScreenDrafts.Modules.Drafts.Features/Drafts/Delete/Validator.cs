namespace ScreenDrafts.Modules.Drafts.Features.Drafts.Delete;

internal sealed class Validator : AbstractValidator<DeleteDraftCommand>
{
  public Validator()
  {
    RuleFor(x => x.PublicId)
      .NotEmpty()
      .WithMessage("Public ID is required.")
      .Must(id => PublicIdGuards.IsValidWithPrefix(id, PublicIdPrefixes.Draft))
      .WithMessage("Public ID must be a valid public ID with the correct prefix.");
  }
}
