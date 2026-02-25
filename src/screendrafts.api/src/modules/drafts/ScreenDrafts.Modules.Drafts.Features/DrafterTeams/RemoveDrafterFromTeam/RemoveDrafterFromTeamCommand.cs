namespace ScreenDrafts.Modules.Drafts.Features.DrafterTeams.RemoveDrafterFromTeam;

internal sealed record RemoveDrafterFromTeamCommand : ICommand
{
  public required string DrafterTeamId { get; init; }
  public required string DrafterId { get; init; }
}
