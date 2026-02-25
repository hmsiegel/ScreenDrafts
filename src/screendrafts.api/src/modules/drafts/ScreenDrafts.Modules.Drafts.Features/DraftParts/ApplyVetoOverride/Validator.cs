namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.ApplyVetoOverride;

internal sealed class Validator : AbstractValidator<ApplyVetoOverrideCommand>
{
  public Validator()
  {
    RuleFor(x => x.DraftPartId)
      .NotEmpty()
      .WithMessage("Draft part ID is required.")
      .Must(id => PublicIdGuards.IsValidWithPrefix(id, PublicIdPrefixes.DraftPart))
      .WithMessage("Draft part ID must be a valid public ID with the correct prefix.");

    RuleFor(x => x.PlayOrder)
      .GreaterThan(0)
      .WithMessage("Play order must be greater than 0.");

    RuleFor(x => x.ParticipantIdValue)
      .NotEmpty()
      .When(x => x.ParticipantKind != ParticipantKind.Community)
      .WithMessage("Participant public ID is required for non-community participants.")
      .Must(id => PublicIdGuards.IsValid(id!))
      .WithMessage("Participant ID must be a valid public ID.")
      .When(x => x.ParticipantKind != ParticipantKind.Community.Value);
  }
}
