namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.Zoom.GetZoomSessionToken;

internal sealed record ZoomSessionTokenResult
{
  public required string SessionName { get; init; }
  public required string Token { get; init; }
}
