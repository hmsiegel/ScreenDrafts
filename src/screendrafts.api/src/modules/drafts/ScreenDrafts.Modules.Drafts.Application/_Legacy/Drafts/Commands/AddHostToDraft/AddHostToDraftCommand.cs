namespace ScreenDrafts.Modules.Drafts.Application._Legacy.Drafts.Commands.AddHostToDraft;
public sealed record AddHostToDraftCommand(Guid DraftPartId, Guid HostId, string Role) : ICommand<Guid>;
