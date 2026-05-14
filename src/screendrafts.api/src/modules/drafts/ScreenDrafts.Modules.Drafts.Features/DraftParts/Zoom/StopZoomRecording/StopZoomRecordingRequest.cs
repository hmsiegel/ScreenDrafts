namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.Zoom.StopZoomRecording;

internal sealed record StopZoomRecordingRequest
{
  [FromRoute(Name = "draftPartId")]
  public string DraftPartId { get; init; } = default!;
}
