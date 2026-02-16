namespace ScreenDrafts.Modules.Drafts.Application._Legacy.Drafts.Commands.AddDrafterTeamToDraft;

public sealed record AddDrafterTeamToDraftCommand(Guid DraftPartId, Guid DrafterTeamId) : ICommand;
