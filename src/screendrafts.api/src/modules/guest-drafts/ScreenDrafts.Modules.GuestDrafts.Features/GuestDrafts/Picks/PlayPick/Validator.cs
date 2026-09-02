namespace ScreenDrafts.Modules.GuestDrafts.Features.GuestDrafts.Picks.PlayPick;

internal sealed class Validator : AbstractValidator<PlayPickCommand>
{
  public Validator()
  {
    RuleFor(x => x.GuestDraftPublicId)
      .NotEmpty()
      .WithMessage("Guest draft public ID is required.")
      .Must(id => PublicIdGuards.IsValidWithPrefix(id, PublicIdPrefixes.GuestDraft))
      .WithMessage("Guest draft public ID must be a valid public ID with the correct prefix.");

    RuleFor(x => x.MoviePublicId)
      .NotEmpty()
      .WithMessage("Movie public ID is required.")
      .Must(id => PublicIdGuards.IsValidWithPrefix(id, PublicIdPrefixes.Media))
      .WithMessage("Movie public ID must be a valid public ID with the correct prefix.");

    RuleFor(x => x.Position).GreaterThan(0).WithMessage("Position must be greater than 0.");
    RuleFor(x => x.PlayOrder).GreaterThan(0).WithMessage("Play order must be greater than 0.");
  }
}
