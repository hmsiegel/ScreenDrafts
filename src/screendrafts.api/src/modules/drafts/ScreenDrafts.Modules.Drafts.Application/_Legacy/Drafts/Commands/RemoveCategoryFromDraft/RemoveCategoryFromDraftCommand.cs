namespace ScreenDrafts.Modules.Drafts.Application._Legacy.Drafts.Commands.RemoveCategoryFromDraft;

public sealed record RemoveCategoryFromDraftCommand(Guid DraftId, Guid CategoryId) : ICommand;
