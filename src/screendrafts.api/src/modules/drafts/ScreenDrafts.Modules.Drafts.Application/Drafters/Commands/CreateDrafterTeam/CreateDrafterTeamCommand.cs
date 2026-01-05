namespace ScreenDrafts.Modules.Drafts.Application.Drafters.Commands.CreateDrafterTeam;

public sealed record CreateDrafterTeamCommand(string TeamName) : ICommand<Guid>;
