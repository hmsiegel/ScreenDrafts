namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.SetDraftPosition;

internal sealed class Validator : AbstractValidator<SetDraftPositionsRequest>
{
  public Validator()
  {
    RuleFor(x => x.DraftPartId)
      .NotEmpty()
      .WithMessage("Draft part ID is required.")
      .Must(id => PublicIdGuards.IsValidWithPrefix(id, PublicIdPrefixes.DraftPart))
      .WithMessage("Draft part ID must be a valid public ID with the correct prefix.");

    RuleFor(x => x.Positions)
      .NotEmpty()
      .WithMessage("Positions are required.")
      .Must(positions => positions.Count > 0)
      .WithMessage("At least one position is required.");

    RuleForEach(x => x.Positions).ChildRules(position =>
    {
      position.RuleFor(p => p.Name)
       .NotEmpty()
       .WithMessage("Position name is required.");

      position.RuleFor(p => p.Picks)
        .NotEmpty()
        .WithMessage("Picks list cannot be empty.");

      position.RuleFor(p => p.Picks)
        .Must(picks => picks.All(p => p > 0))
        .WithMessage("All pick numbers must be greater than zero.");
    });
  }
}
