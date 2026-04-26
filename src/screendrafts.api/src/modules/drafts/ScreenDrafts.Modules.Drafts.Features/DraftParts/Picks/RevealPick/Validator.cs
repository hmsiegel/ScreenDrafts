namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.Picks.RevealPick;

internal sealed class Validator : AbstractValidator<RevealPickCommand>
{
  public Validator()
  {
    RuleFor(x => x.DraftPartId)
      .NotEmpty()
      .WithMessage("Draft part ID is required.")
      .Must(id => PublicIdGuards.IsValidWithPrefix(id, PublicIdPrefixes.DraftPart))
      .WithMessage("Draft part ID must be a valid public ID with the correct prefix.");

    RuleFor(x => x.PlayOrder)
      .GreaterThan(0)
      .WithMessage("Play order must be greater than 0.");
  }
}
