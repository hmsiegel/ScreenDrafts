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
      .When(x => x.ParticipantKind != ParticipantKind.Community);

    RuleFor(x => x.ParticipantKind)
      .Must(x => ParticipantKind.List.Any(pk => pk.Value == x.Value))
      .WithMessage("Invalid participant kind. Valid values are: " + string.Join(", ", SmartEnum<ParticipantKind>.List));

    RuleFor(x => x.MovieId)
      .NotEmpty();
  }
}
