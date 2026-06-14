namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.GamePlay.GetDraftPartGamePlay;

internal sealed record GameplayHostResponse
{
  public string HostPublicId { get; init; } = default!;
  public string HostName { get; init; } = default!;
  public bool IsPrimary { get; init; }
}
