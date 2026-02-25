namespace ScreenDrafts.Modules.Drafts.Features.DrafterTeams.AddDrafterToTeam;

internal sealed record AddDrafterToTeamCommand : ICommand
{
  public required string DrafterTeamId { get; init; }
  public required string DrafterId { get; init; }
}
