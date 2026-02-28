namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.AddParticipantToDraftPart;

internal sealed class Validator : AbstractValidator<AddParticipantToDraftPartCommand>
{
  public Validator()
  {
    RuleFor(x => x.DraftPartPublicId).NotEmpty().WithMessage("Draft part ID is required.");

    RuleFor(x => x.ParticipantPublicId)
      .NotEmpty()
      .When(x => x.ParticipantKind != ParticipantKind.Community)
      .WithMessage("Participant public ID is required for non-community participants.")
      .Must(id => PublicIdGuards.IsValid(id!))
      .WithMessage("Participant ID must be a valid public ID.")
      .When(x => x.ParticipantKind != ParticipantKind.Community.Value);
  }
}

