namespace ScreenDrafts.Modules.Drafts.Application._Legacy.Drafts.Commands.AddDrafterToDraft;
public sealed record AddDrafterToDraftCommand(Guid DraftPartId, Guid DrafterId) : ICommand<Guid>;
