namespace ScreenDrafts.Modules.Drafts.Features.DrafterTeams.AddDrafterToTeam;

internal sealed record AddDrafterToTeamRequest
{
  [FromRoute(Name = "publicId")]
  public required string DrafterTeamId { get; init; }

  public required string DrafterId { get; init; }
}
