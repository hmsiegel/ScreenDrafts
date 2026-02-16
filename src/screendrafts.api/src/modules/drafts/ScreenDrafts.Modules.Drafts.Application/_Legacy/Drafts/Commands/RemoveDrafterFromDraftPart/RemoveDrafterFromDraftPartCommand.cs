namespace ScreenDrafts.Modules.Drafts.Application._Legacy.Drafts.Commands.RemoveDrafterFromDraftPart;

public sealed record RemoveDrafterFromDraftPartCommand(Guid DraftPartId, Guid DrafterId) : ICommand<Guid>;
