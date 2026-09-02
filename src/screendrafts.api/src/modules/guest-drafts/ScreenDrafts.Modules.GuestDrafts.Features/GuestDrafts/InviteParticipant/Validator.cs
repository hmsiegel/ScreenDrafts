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

    RuleFor(x => x.InviteeUserPublicId)
      .NotEmpty()
      .WithMessage("Invitee user public ID is required.")
      .Must(id => PublicIdGuards.IsValidWithPrefix(id, PublicIdPrefixes.User))
      .WithMessage("Invitee user public ID is invalid.");

    RuleFor(x => x.CallerUserPublicId)
      .NotEmpty()
      .WithMessage("Caller user public ID is required.")
      .Must(id => PublicIdGuards.IsValidWithPrefix(id, PublicIdPrefixes.User))
      .WithMessage("Caller user public ID is invalid.");
  }
}
