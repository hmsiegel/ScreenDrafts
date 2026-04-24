namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.Zoom.StartZoomSession;

internal sealed record StartZoomSessionResult
{
  public required string SessionName { get; init; }
  public required string Token { get; init; }
}
