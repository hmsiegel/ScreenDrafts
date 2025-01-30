namespace ScreenDrafts.Modules.Drafts.Application.Drafters.Commands.CreateDrafter;
public sealed record CreateDrafterCommand(string Name, Guid UserId) : ICommand;
