
namespace ScreenDrafts.Modules.Drafts.Application.Drafts.Commands.RemoveCategoryFromDraft;

public sealed record RemoveCategoryFromDraftCommand(Guid DraftId, Guid CategoryId) : ICommand;
