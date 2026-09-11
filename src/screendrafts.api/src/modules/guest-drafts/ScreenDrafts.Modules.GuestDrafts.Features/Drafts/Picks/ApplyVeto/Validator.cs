using ScreenDrafts.Modules.GuestDrafts.Features.Drafts.Picks.ApplyVeto;

namespace ScreenDrafts.Modules.GuestDrafts.Features.Drafts.Picks.ApplyVeto;

internal sealed class Validator : AbstractValidator<ApplyVetoCommand>
{
  public Validator()
  {
    RuleFor(x => x.GuestDraftPublicId)
      .NotEmpty()
      .WithMessage("Guest draft public id is required.")
      .Must(id => PublicIdGuards.IsValidWithPrefix(id, PublicIdPrefixes.GuestDraft))
      .WithMessage("Guest draft public id is invalid.");
    RuleFor(x => x.PlayOrder).GreaterThan(0).WithMessage("Play order must be a positive number.");
  }
}
