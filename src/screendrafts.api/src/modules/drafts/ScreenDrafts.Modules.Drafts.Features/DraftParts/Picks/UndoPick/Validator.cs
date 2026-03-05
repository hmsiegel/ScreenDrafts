namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.Picks.UndoPick;

internal sealed class Validator : AbstractValidator<UndoPickCommand>
{
  public Validator()
  {
    RuleFor(x => x.DraftPartPublicId)
      .NotEmpty()
      .WithMessage("Draft part public ID is required.")
      .Must(id => PublicIdGuards.IsValidWithPrefix(id, PublicIdPrefixes.DraftPart))
      .WithMessage("Invalid draft part public ID format.");

    RuleFor(x => x.PlayOrder)
      .GreaterThan(0)
      .WithMessage("Play order must be greater than 0.");
  }
}
