namespace ScreenDrafts.Modules.Drafts.Application.Drafts.Commands.PauseDraft;

public sealed record PauseDraftCommand(Guid DraftId) : ICommand;
