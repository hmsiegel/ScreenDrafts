namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.Zoom.StopZoomRecording;

internal sealed record StopZoomRecordingCommand : ICommand
{
  public required string DraftPartPublicId { get; init; }
}
