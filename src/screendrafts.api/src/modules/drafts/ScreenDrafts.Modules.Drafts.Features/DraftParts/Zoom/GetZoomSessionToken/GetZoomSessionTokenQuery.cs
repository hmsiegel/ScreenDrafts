namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.Zoom.GetZoomSessionToken;

internal sealed record GetZoomSessionTokenQuery : IQuery<ZoomSessionTokenResult>
{
  public required string DraftPartPublicId { get; init; }
  public required string ParticipantPublicId { get; init; }
}
