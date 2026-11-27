namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.Participants.SetPhantomParticipant;

internal sealed class Validator : AbstractValidator<SetPhantomParticipantCommand>
{
  public Validator()
  {
    RuleFor(x => x.DraftPartId)
      .NotEmpty()
      .WithMessage("Draft part public ID is required.")
      .Must(id => PublicIdGuards.IsValidWithPrefix(id, PublicIdPrefixes.DraftPart))
      .WithMessage("Draft part ID must have a valid prefix.");

    RuleFor(x => x.ParticipantPublicId)
      .NotEmpty()
      .WithMessage("Participant public ID is required.")
      .Must(id => PublicIdGuards.IsValid(id))
      .WithMessage("Participant ID must be a valid public ID.");
  }
}
