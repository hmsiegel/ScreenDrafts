namespace ScreenDrafts.Modules.Drafts.Features.WikiExport.ExportDrafters;

internal sealed record ExportDraftersWikiQuery : IQuery<ExportWikiResponse>
{
  public required IReadOnlyList<string> DrafterPublicIds { get; init; }
}
