namespace ScreenDrafts.Modules.Drafts.Features.DrafterTeams.UpdateDrafterTeamName;

internal sealed record UpdateDrafterTeamNameCommand : ICommand
{
  public required string DrafterTeamId { get; init; }
  public required string Name { get; init; }
}
