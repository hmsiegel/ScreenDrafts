namespace ScreenDrafts.Modules.Drafts.Application.Drafters.Commands.AddDrafterToDrafterTeam;

public sealed record AddDrafterToDrafterTeamCommand(Guid DrafterId, Guid DrafterTeamId) : ICommand;
