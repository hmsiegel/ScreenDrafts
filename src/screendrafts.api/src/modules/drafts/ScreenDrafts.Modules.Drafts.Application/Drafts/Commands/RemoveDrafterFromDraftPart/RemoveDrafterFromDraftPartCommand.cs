namespace ScreenDrafts.Modules.Drafts.Application.Drafts.Commands.RemoveDrafterFromDraftPart;

public sealed record RemoveDrafterFromDraftPartCommand(Guid DraftPartId, Guid DrafterId) : ICommand<Guid>;
