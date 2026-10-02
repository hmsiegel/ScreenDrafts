namespace ScreenDrafts.Modules.Drafts.Features.WikiExport.ExportDrafts;

internal sealed record ExportDraftsWikiQuery : IQuery<ExportWikiResponse>
{
  public required IReadOnlyList<string> DraftPublicIds { get; init; }
}
