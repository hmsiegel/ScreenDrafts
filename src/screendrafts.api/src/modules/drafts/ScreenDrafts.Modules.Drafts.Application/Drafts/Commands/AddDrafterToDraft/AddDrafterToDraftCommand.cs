namespace ScreenDrafts.Modules.Drafts.Application.Drafts.Commands.AddDrafterToDraft;
public sealed record AddDrafterToDraftCommand(Guid DraftPartId, Guid DrafterId) : ICommand<Guid>;
