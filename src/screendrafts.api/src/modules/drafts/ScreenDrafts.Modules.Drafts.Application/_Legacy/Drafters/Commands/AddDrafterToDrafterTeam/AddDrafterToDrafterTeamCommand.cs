namespace ScreenDrafts.Modules.Drafts.Application._Legacy.Drafters.Commands.AddDrafterToDrafterTeam;

public sealed record AddDrafterToDrafterTeamCommand(Guid DrafterId, Guid DrafterTeamId) : ICommand;
