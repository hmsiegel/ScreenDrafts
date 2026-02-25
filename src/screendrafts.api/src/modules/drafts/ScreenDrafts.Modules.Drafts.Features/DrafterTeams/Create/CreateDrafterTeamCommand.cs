namespace ScreenDrafts.Modules.Drafts.Features.DrafterTeams.Create;

internal class CreateDrafterTeamCommand : ICommand<string>
{
  public required string Name { get; init; }
}
