namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.Zoom.StartZoomRecording;

internal sealed record StartZoomRecordingRequest
{
  [FromRoute(Name = "draftPartId")]
  public string DraftPartId { get; init; } = default!;
}
