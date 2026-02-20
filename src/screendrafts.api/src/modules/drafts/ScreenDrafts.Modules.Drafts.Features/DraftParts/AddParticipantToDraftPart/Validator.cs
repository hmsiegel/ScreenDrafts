namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.AddParticipantToDraftPart;

internal sealed class Validator : AbstractValidator<AddParticipantToDraftPartCommand>
{
  public Validator()
  {
    RuleFor(x => x.DraftPartId).NotEmpty().WithMessage("Draft part ID is required.");
    RuleFor(x => x.ParticipantPublicId)
      .NotEmpty()
      .When(x => x.ParticipantKind != ParticipantKind.Community)
      .WithMessage("Participant public ID is required for non-community participants.");
    RuleFor(x => x.ParticipantKind)
      .Must(k => ParticipantKind.List.Any(pk => pk.Value == k.Value))
      .WithMessage("Invalid participant kind. Valid values are: " + string.Join(", ", SmartEnum<ParticipantKind>.List) + ")");
  }
}

