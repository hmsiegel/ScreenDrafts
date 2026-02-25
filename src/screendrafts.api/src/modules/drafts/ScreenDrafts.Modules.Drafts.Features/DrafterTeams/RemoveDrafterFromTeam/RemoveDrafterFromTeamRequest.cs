namespace ScreenDrafts.Modules.Drafts.Features.DrafterTeams.RemoveDrafterFromTeam;

internal sealed record RemoveDrafterFromTeamRequest
{
  [FromRoute(Name = "publicId")]
  public required string DrafterTeamId { get; init; }

  [FromRoute(Name = "drafterId")]
  public required string DrafterId { get; init; }
}
