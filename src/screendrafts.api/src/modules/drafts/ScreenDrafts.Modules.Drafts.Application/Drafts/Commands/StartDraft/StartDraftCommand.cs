namespace ScreenDrafts.Modules.Drafts.Application.Drafts.Commands.StartDraft;

public sealed record StartDraftCommand(Guid DraftId) : ICommand;
