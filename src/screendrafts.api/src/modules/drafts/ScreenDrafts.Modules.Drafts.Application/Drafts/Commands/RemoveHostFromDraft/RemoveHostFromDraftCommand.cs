namespace ScreenDrafts.Modules.Drafts.Application.Drafts.Commands.RemoveHostFromDraft;

public sealed record RemoveHostFromDraftCommand(Guid DraftId, Guid HostId) : ICommand<Guid>;
