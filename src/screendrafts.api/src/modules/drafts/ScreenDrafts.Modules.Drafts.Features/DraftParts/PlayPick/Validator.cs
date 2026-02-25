namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.PlayPick;

internal sealed class Validator : AbstractValidator<PlayPickCommand>
{
  public Validator()
  {
    RuleFor(x => x.DraftPartId)
      .NotEmpty();

    RuleFor(x => x.Position)
      .GreaterThanOrEqualTo(0);

    RuleFor(x => x.PlayOrder)
      .GreaterThanOrEqualTo(0);

    RuleFor(x => x.ParticipantPublicId)
      .NotEmpty()
      .When(x => x.ParticipantKind != ParticipantKind.Community)
      .WithMessage("Participant public ID is required for non-community participants.")
      .Must(id => PublicIdGuards.IsValid(id!))
      .WithMessage("Participant ID must be a valid public ID.")
      .When(x => x.ParticipantKind != ParticipantKind.Community.Value);

    RuleFor(x => x.MovieId)
      .NotEmpty();
  }
}
