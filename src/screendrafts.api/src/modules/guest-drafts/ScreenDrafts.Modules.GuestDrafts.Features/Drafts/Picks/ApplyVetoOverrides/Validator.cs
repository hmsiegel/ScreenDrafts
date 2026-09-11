using ScreenDrafts.Modules.GuestDrafts.Features.Drafts.Picks.ApplyVetoOverrides;

namespace ScreenDrafts.Modules.GuestDrafts.Features.Drafts.Picks.ApplyVetoOverrides;

internal sealed class Validator : AbstractValidator<ApplyVetoOverrideCommand>
{
  public Validator()
  {
    RuleFor(x => x.GuestDraftPublicId)
      .NotEmpty()
      .WithMessage("Guest draft public ID is required.")
      .Must(id => PublicIdGuards.IsValidWithPrefix(id, PublicIdPrefixes.GuestDraft))
      .WithMessage("Guest draft public ID is invalid.");
    RuleFor(x => x.PlayOrder).GreaterThan(0).WithMessage("Play order must be greater than 0.");
  }
}
