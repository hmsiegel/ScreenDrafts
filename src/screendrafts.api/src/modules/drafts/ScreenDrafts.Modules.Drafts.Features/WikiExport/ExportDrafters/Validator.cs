namespace ScreenDrafts.Modules.Drafts.Features.WikiExport.ExportDrafters;

// ── Validator ─────────────────────────────────────────────────────────────

internal sealed class Validator : AbstractValidator<ExportDraftersWikiQuery>
{
  public Validator()
  {
    RuleFor(x => x.DrafterPublicIds)
      .NotEmpty()
      .WithMessage("Select at least one drafter.")
      .Must(ids => ids.Count <= WikiText.MaxSelection)
      .WithMessage($"Select at most {WikiText.MaxSelection} drafters per export.");

    RuleForEach(x => x.DrafterPublicIds)
      .Must(id => PublicIdGuards.IsValidWithPrefix(id, PublicIdPrefixes.Drafter))
      .WithMessage("Drafter public ID is invalid.");
  }
}
