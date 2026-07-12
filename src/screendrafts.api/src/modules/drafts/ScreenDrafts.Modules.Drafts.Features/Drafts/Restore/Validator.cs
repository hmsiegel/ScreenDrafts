namespace ScreenDrafts.Modules.Drafts.Features.Drafts.Restore;

internal sealed class Validator : AbstractValidator<RestoreDraftCommand>
{
  public Validator()
  {
    RuleFor(x => x.PublicId)
      .NotEmpty()
      .WithMessage("PublicId is required.")
      .Must(id => PublicIdGuards.IsValidWithPrefix(id, PublicIdPrefixes.Draft))
      .WithMessage("PublicId must be a valid draft ID with the correct prefix.");
  }
}
