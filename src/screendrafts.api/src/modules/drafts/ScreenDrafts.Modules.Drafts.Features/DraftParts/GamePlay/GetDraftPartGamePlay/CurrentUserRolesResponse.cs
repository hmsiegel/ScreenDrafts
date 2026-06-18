namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.GamePlay.GetDraftPartGamePlay;

internal sealed record CurrentUserRolesResponse
{
  public bool IsPrimaryHost { get; init; }
  public bool IsCoHost { get; init; }
  public bool IsParticipant { get; init; }
  public bool IsCommissioner { get; init; }
}
