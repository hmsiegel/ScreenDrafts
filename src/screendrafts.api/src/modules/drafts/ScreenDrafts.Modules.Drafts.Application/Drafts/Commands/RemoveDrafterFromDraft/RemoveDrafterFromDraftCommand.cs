namespace ScreenDrafts.Modules.Drafts.Application.Drafts.Commands.RemoveDrafterFromDraft;

public sealed record RemoveDrafterFromDraftCommand(Guid DraftId, Guid DrafterId) : ICommand<Guid>;
