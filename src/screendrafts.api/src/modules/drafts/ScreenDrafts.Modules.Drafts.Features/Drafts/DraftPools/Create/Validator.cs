namespace ScreenDrafts.Modules.Drafts.Features.Drafts.DraftPools.Create;

internal sealed class Validator : AbstractValidator<CreateDraftPoolCommand>
{
  public Validator()
  {
    RuleFor(x => x.PublicId)
      .NotEmpty()
      .WithMessage("Public ID is required.")
      .MaximumLength(PublicIdPrefixes.MaxPublicIdLength)
      .WithMessage($"Public ID must not exceed {PublicIdPrefixes.MaxPublicIdLength} characters.")
      .Must(id => PublicIdGuards.IsValidWithPrefix(id, PublicIdPrefixes.Draft))
      .WithMessage("Invalid draft public ID.");
  }
}
