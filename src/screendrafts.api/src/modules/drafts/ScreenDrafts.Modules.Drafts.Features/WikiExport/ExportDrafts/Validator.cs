// Drafts module — Features/WikiExport/ExportDrafts/ExportDraftsWiki.cs
// Split into Validator.cs, ExportDraftsWikiRequest.cs, ExportDraftsWikiQuery.cs,
// ExportDraftsWikiQueryHandler.cs, Validator.cs. All share this namespace.

namespace ScreenDrafts.Modules.Drafts.Features.WikiExport.ExportDrafts;

// ── Validator ─────────────────────────────────────────────────────────────

internal sealed class Validator : AbstractValidator<ExportDraftsWikiQuery>
{
  public Validator()
  {
    RuleFor(x => x.DraftPublicIds)
      .NotEmpty()
      .WithMessage("Select at least one draft.")
      .Must(ids => ids is null || ids.Count <= WikiText.MaxSelection)
      .WithMessage($"Select at most {WikiText.MaxSelection} drafts per export.");

    RuleForEach(x => x.DraftPublicIds)
      .Must(id => PublicIdGuards.IsValidWithPrefix(id, PublicIdPrefixes.Draft))
      .WithMessage("Draft public ID is invalid.");
  }
}
