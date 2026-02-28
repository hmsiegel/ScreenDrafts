namespace ScreenDrafts.Modules.Drafts.Features.Drafts.CreateDraftPart;

internal sealed class Validator : AbstractValidator<CreateDraftPartCommand>
{
  public Validator()
  {
    RuleFor(x => x.DraftPublicId)
      .NotEmpty()
      .Must(x => PublicIdGuards.IsValidWithPrefix(x, PublicIdPrefixes.Draft))
      .WithMessage("Draft ID is required.");
  }
}
