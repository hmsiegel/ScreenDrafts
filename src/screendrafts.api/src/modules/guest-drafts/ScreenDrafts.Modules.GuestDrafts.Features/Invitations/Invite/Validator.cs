using ScreenDrafts.Modules.GuestDrafts.Domain.Invitations;

namespace ScreenDrafts.Modules.GuestDrafts.Features.Invitations.Invite;

// ── Validator ─────────────────────────────────────────────────────────────────
internal sealed class Validator : AbstractValidator<InviteGuestDrafterCommand>
{
  public Validator()
  {
    RuleFor(x => x.GuestDraftPublicId)
      .NotEmpty()
      .WithMessage("Guest draft public ID is required.")
      .Must(id => PublicIdGuards.IsValidWithPrefix(id, PublicIdPrefixes.GuestDraft))
      .WithMessage("Invalid guest draft public ID format.");

    RuleFor(x => x.Email)
      .NotEmpty()
      .MaximumLength(Invitation.EmailMaxLength)
      .EmailAddress()
      .WithMessage("A valid email address is required.");

    RuleFor(x => x.Name).NotEmpty().MaximumLength(Invitation.NameMaxLength);
  }
}
