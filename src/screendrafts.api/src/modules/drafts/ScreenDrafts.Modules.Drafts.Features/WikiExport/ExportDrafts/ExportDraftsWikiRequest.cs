namespace ScreenDrafts.Modules.Drafts.Features.WikiExport.ExportDrafts;

internal sealed record ExportDraftsWikiRequest
{
  public required IReadOnlyList<string> DraftPublicIds { get; init; }
}
