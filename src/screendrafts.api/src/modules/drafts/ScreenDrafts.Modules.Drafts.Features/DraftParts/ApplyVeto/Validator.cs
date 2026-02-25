namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.ApplyVeto;

internal sealed class Validator : AbstractValidator<ApplyVetoCommand>
{
  public Validator()
  {
    RuleFor(x => x.DraftPartId)
      .NotEmpty().WithMessage("Draft part ID is required.")
      .Must(id => PublicIdGuards.IsValidWithPrefix(id, PublicIdPrefixes.DraftPart))
      .WithMessage("Draft part ID must be a valid public ID with the correct prefix.");

    RuleFor(x => x.PlayOrder)
      .GreaterThan(0).WithMessage("Play order must be greater than 0.");

    RuleFor(x => x.ParticipantPublicId)
      .Must(id => PublicIdGuards.IsValid(id))
      .WithMessage("Participant public ID must be a valid public ID.");
  }
}
