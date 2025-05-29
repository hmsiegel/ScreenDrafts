namespace ScreenDrafts.Modules.Drafts.Application.Drafts.Commands.ContinueDraft;

public sealed record ContinueDraftCommand(Guid DraftId) : ICommand;
