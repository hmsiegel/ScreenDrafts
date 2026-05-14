namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.Zoom.EndZoomSession;

internal sealed record EndZoomSessionRequest
{
  [FromRoute(Name = "draftPartId")]
  public string DraftPartId { get; init; } = default!;
}
