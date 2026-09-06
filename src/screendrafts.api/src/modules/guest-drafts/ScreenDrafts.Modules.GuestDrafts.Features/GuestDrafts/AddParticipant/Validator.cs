namespace ScreenDrafts.Modules.GuestDrafts.Features.GuestDrafts.AddParticipant;

internal sealed class Validator : AbstractValidator<AddParticipantCommand>
{
  public Validator()
  {
    RuleFor(x => x.GuestDraftPublicId)
      .NotEmpty()
      .WithMessage("Guest draft public ID is required.")
      .Must(id => PublicIdGuards.IsValidWithPrefix(id, PublicIdPrefixes.GuestDraft))
      .WithMessage("Invalid guest draft public ID format.");
    RuleFor(x => x.GuestDrafterPublicId)
      .NotEmpty()
      .WithMessage("Guest drafter public ID is required.")
      .Must(id => PublicIdGuards.IsValidWithPrefix(id, PublicIdPrefixes.GuestDrafter))
      .WithMessage("Invalid guest drafter public ID format.");
  }
}
