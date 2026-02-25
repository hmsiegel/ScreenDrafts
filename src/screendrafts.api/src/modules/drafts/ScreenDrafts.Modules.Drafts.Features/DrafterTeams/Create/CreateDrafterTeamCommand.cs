namespace ScreenDrafts.Modules.Drafts.Features.DrafterTeams.Create;

internal sealed class CreateDrafterTeamCommand : ICommand<string>
{
  public required string Name { get; init; }
}
