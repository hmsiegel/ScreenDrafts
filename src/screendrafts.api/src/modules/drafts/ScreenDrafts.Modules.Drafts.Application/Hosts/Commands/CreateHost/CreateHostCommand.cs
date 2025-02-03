namespace ScreenDrafts.Modules.Drafts.Application.Hosts.Commands.CreateHost;
public sealed record CreateHostCommand(Guid UserId) : ICommand<Guid>;
