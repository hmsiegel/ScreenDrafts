namespace ScreenDrafts.Modules.Drafts.Application.Drafts.Commands.DeleteDraft;

public sealed record DeleteDraftCommand(Guid DraftId) : ICommand;
