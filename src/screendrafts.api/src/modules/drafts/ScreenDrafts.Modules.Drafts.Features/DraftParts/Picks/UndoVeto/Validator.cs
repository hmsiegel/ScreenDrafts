namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.Picks.UndoVeto;

// ── Validator ─────────────────────────────────────────────────────────────────

internal sealed class Validator : AbstractValidator<UndoVetoCommand>
{
  public Validator()
  {
    RuleFor(x => x.DraftPartId)
      .NotEmpty()
      .WithMessage("Draft part ID is required.")
      .Must(id => PublicIdGuards.IsValidWithPrefix(id, PublicIdPrefixes.DraftPart))
      .WithMessage("Invalid draft part ID format.");
    RuleFor(x => x.PlayOrder).GreaterThan(0).WithMessage("Play order must be greater than 0.");
  }
}
