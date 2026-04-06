namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.SubDrafts.PlaySubDraftPick;

internal sealed class Validator : AbstractValidator<PlaySubDraftPickCommand>
{
  public Validator()
  {
    RuleFor(x => x.DraftPartPublicId)
      .NotEmpty()
      .WithMessage("Draft part public ID is required.")
      .Must(id => PublicIdGuards.IsValidWithPrefix(id, PublicIdPrefixes.DraftPart))
      .WithMessage("Draft part public ID must be a valid public ID with the correct prefix.");

    RuleFor(x => x.SubDraftPublicId)
      .NotEmpty()
      .WithMessage("Sub-draft public ID is required.")
      .Must(id => PublicIdGuards.IsValidWithPrefix(id, PublicIdPrefixes.SubDraft))
      .WithMessage("Sub-draft public ID must be a valid public ID with the correct prefix.");

    RuleFor(x => x.MoviePublicId)
      .NotEmpty()
      .WithMessage("Movie public ID is required.")
      .Must(id => PublicIdGuards.IsValidWithPrefix(id, PublicIdPrefixes.Media))
      .WithMessage("Movie public ID must be a valid public ID with the correct prefix.");

    RuleFor(x => x.Position)
      .GreaterThan(0)
      .WithMessage("Position must be greater than 0.");

    RuleFor(x => x.PlayOrder)
      .GreaterThan(0)
      .WithMessage("Play order must be greater than 0.");

    RuleFor(x => x.ParticipantPublicId)
      .NotEmpty()
      .WithMessage("Participant public ID is required.");
  }
}
