namespace ScreenDrafts.Modules.Drafts.Application.Drafts.Commands.AddHostToDraft;
public sealed record AddHostToDraftCommand(Guid DraftId, Guid HostId, string Role) : ICommand<Guid>;
