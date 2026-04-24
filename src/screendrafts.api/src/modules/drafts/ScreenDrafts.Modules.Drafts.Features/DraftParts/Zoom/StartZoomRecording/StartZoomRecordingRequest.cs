namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.Zoom.StartZoomRecording;

internal sealed record StartZoomRecordingRequest
{
  [FromRoute(Name = "draftPartId")]
  public required string DraftPartId { get; init; }
}
