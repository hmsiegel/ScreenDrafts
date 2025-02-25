namespace ScreenDrafts.Modules.Drafts.Application.Drafts.Commands.CompleteDraft;

public sealed record CompleteDraftCommand(Guid DraftId) : ICommand;
