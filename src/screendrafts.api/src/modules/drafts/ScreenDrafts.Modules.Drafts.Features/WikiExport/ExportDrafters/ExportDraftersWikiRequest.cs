namespace ScreenDrafts.Modules.Drafts.Features.WikiExport.ExportDrafters;

// ── Request / query ───────────────────────────────────────────────────────

internal sealed record ExportDraftersWikiRequest
{
  public required IReadOnlyList<string> DrafterPublicIds { get; init; }
}
