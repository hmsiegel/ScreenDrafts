namespace ScreenDrafts.Modules.Drafts.Application.Hosts.Commands.CreateHostWithoutUser;

public sealed record CreateHostWithoutUserCommand(string Name) : ICommand<Guid>;
