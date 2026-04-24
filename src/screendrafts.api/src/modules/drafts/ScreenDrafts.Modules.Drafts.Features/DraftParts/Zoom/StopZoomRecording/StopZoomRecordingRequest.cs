namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.Zoom.StopZoomRecording;

internal sealed record StopZoomRecordingRequest
{
  [FromRoute(Name = "draftPartId")]
  public required string DraftPartId { get; init; }
}
