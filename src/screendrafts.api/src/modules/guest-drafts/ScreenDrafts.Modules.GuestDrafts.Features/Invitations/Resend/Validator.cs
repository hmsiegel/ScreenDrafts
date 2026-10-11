namespace ScreenDrafts.Modules.GuestDrafts.Features.Invitations.Resend;

// ── Validator ─────────────────────────────────────────────────────────────────
internal sealed class Validator : AbstractValidator<ResendGuestDraftInvitationCommand>
{
  public Validator()
  {
    RuleFor(x => x.GuestDraftPublicId)
      .NotEmpty()
      .WithMessage("Guest draft public ID is required.")
      .Must(id => PublicIdGuards.IsValidWithPrefix(id, PublicIdPrefixes.GuestDraft))
      .WithMessage("Invalid guest draft public ID format.");

    RuleFor(x => x.InvitationPublicId)
      .NotEmpty()
      .WithMessage("Invitation public ID is required.")
      .Must(id => PublicIdGuards.IsValidWithPrefix(id, PublicIdPrefixes.GuestDraftInvitation))
      .WithMessage("Invalid invitation public ID format.");
  }
}
