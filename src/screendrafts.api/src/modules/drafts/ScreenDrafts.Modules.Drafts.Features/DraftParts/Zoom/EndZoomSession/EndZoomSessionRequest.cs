namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.Zoom.EndZoomSession;

internal sealed record EndZoomSessionRequest
{
  [FromRoute(Name = "draftPartId")]
  public required string DraftPartId { get; init; }
}
