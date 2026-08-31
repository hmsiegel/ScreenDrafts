namespace ScreenDrafts.Modules.GuestDrafts.Features.GuestDrafts.InviteParticipant;

internal sealed class Validator : AbstractValidator<InviteParticipantCommand>
{
  public Validator()
  {
    RuleFor(x => x.GuestDraftPublicId)
      .NotEmpty()
      .WithMessage("Guest draft public ID is required.")
      .Must(id => PublicIdGuards.IsValidWithPrefix(id, PublicIdPrefixes.GuestDraft))
      .WithMessage("Guest draft public ID is invalid.");


    RuleFor(x => x.UserPublicId)
      .NotEmpty()
      .WithMessage("User public ID is required.")
      .Must(id => PublicIdGuards.IsValidWithPrefix(id, PublicIdPrefixes.User))
      .WithMessage("User public ID is invalid.");
  }
}
