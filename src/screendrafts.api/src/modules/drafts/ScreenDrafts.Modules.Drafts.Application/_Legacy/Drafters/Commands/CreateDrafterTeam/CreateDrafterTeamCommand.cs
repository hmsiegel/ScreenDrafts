namespace ScreenDrafts.Modules.Drafts.Application._Legacy.Drafters.Commands.CreateDrafterTeam;

public sealed record CreateDrafterTeamCommand(string TeamName) : ICommand<Guid>;
