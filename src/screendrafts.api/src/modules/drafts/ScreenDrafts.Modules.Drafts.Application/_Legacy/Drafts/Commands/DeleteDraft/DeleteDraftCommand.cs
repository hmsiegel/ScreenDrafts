namespace ScreenDrafts.Modules.Drafts.Application._Legacy.Drafts.Commands.DeleteDraft;

public sealed record DeleteDraftCommand(Guid DraftId) : ICommand;
