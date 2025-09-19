namespace ScreenDrafts.Modules.Drafts.Application.Drafts.Commands.AddDrafterTeamToDraft;

public sealed record AddDrafterTeamToDraftCommand(Guid DraftId, Guid DrafterTeamId) : ICommand;
