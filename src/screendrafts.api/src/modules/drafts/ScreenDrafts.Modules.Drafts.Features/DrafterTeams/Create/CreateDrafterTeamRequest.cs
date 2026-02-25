namespace ScreenDrafts.Modules.Drafts.Features.DrafterTeams.Create;

internal sealed record CreateDrafterTeamRequest
{
  public required string Name { get; init; }
}
