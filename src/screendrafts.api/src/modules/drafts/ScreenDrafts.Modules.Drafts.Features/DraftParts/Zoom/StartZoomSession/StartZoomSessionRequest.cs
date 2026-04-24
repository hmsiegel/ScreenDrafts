namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.Zoom.StartZoomSession;

internal sealed record StartZoomSessionRequest
{
  [FromRoute(Name = "draftPartId")]
  public required string DraftPartId { get; init; }
}
