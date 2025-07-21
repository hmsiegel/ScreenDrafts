namespace ScreenDrafts.Modules.Drafts.Application.Hosts.Commands.CreateHost;
public sealed record CreateHostCommand(Guid PersonId) : ICommand<Guid>;
