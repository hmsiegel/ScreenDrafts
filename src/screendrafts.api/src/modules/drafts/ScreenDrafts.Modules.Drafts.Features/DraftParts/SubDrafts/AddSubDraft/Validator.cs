namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.SubDrafts.AddSubDraft;

internal sealed class Validator : AbstractValidator<AddSubDraftRequest>
{
  public Validator()
  {
    RuleFor(x => x.DraftPartPublicId)
      .NotEmpty()
      .WithMessage("Draft part public ID is required.")
      .Must(id => PublicIdGuards.IsValidWithPrefix(id, PublicIdPrefixes.DraftPart))
      .WithMessage("Draft part public ID must be a valid public ID with the correct prefix.");

    RuleFor(x => x.Index)
      .Equal(3)
      .WithMessage("Index must be equal to 3.");
  }
}
