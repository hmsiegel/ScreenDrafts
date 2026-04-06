namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.SubDrafts.AdvanceSubDraft;

internal sealed class Validator : AbstractValidator<AdvanceSubDraftCommand>
{
  public Validator()
  {
    RuleFor(x => x.DraftPartPublicId)
      .NotEmpty()
      .WithMessage("Draft part public ID is required.")
      .Must(x => PublicIdGuards.IsValidWithPrefix(x, PublicIdPrefixes.DraftPart))
      .WithMessage("Draft part public ID must start with the correct prefix.");

    RuleFor(x => x.SubDraftPublicId)
      .NotEmpty()
      .WithMessage("Sub-draft public ID is required.")
      .Must(x => PublicIdGuards.IsValidWithPrefix(x, PublicIdPrefixes.SubDraft))
      .WithMessage("Sub-draft public ID must start with the correct prefix.");
  }
}

