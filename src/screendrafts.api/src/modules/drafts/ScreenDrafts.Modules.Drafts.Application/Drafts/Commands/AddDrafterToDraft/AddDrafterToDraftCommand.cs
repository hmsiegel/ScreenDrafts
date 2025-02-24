namespace ScreenDrafts.Modules.Drafts.Application.Drafts.Commands.AddDrafterToDraft;
public sealed record AddDrafterToDraftCommand(Guid DraftId, Guid DrafterId) : ICommand<Guid>;
