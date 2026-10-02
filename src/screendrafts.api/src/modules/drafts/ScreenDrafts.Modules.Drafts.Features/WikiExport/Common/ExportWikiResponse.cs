// Drafts module — Features/WikiExport/WikiExport.Common.cs
// Shared by ExportDrafts and ExportDrafters.

namespace ScreenDrafts.Modules.Drafts.Features.WikiExport.Common;

public sealed record ExportWikiResponse
{
  public required string FileName { get; init; }
  public required string Content { get; init; }
  public required int PageCount { get; init; }
}
