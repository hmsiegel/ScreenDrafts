namespace ScreenDrafts.Modules.Drafts.Features.DrafterTeams.RemoveDrafterFromTeam;

internal sealed record RemoveDrafterFromTeamRequest
{
  [FromRoute(Name = "publicId")]
  public string DrafterTeamId { get; init; } = default!;

  [FromRoute(Name = "drafterId")]
  public string DrafterId { get; init; } = default!;
}
