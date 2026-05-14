namespace ScreenDrafts.Modules.Drafts.Features.DrafterTeams.AddDrafterToTeam;

internal sealed record AddDrafterToTeamRequest
{
  [FromRoute(Name = "publicId")]
  public string DrafterTeamId { get; init; } = default!;

  public required string DrafterId { get; init; }
}
