namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.DraftPositions.AssignParticipantToDraftPosition;

internal sealed class Validator : AbstractValidator<AssignParticipantToDraftPositionCommand>
{
  public Validator()
  {
    RuleFor(x => x.DraftPartId)
      .NotEmpty().WithMessage("Draft part ID is required.")
      .Must(id => PublicIdGuards.IsValidWithPrefix(id, PublicIdPrefixes.DraftPart))
      .WithMessage("Invalid draft part ID format.");

    RuleFor(x => x.PositionPublicId)
      .NotEmpty().WithMessage("Position public ID is required.")
      .Must(id => PublicIdGuards.IsValidWithPrefix(id, PublicIdPrefixes.DraftPosition))
      .WithMessage("Invalid position public ID format.");

    RuleFor(x => x.ParticipantPublicId)
      .NotEmpty()
      .When(x => x.ParticipantKind != ParticipantKind.Community)
      .WithMessage("Participant public ID is required for non-community participants.")
      .Must(id => PublicIdGuards.IsValid(id!))
      .WithMessage("Participant ID must be a valid public ID.")
      .When(x => x.ParticipantKind != ParticipantKind.Community.Value);
  }
}
