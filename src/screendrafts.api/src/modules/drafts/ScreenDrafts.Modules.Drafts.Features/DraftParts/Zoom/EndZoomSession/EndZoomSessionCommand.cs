namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.Zoom.EndZoomSession;

internal sealed record EndZoomSessionCommand : ICommand
{
  public required string DraftPartPublicId { get; init; }
}
