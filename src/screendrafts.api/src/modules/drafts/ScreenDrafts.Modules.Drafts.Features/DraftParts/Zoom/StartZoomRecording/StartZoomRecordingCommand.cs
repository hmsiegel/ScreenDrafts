namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.Zoom.StartZoomRecording;

internal sealed record StartZoomRecordingCommand : ICommand
{
  public required string DraftPartPublicId { get; init; }
}
