namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.Zoom.StartZoomSession;

internal sealed record StartZoomSessionCommand : ICommand<StartZoomSessionResult>
{
  public required string DraftPartPublicId { get; init; }
  public required string HostPublicId { get; init; }
}
