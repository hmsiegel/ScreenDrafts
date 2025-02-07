namespace ScreenDrafts.Modules.Drafts.Application.Drafters.Commands.CreateDrafter;
public sealed record CreateDrafterCommand(Guid? UserId, string? Name) : ICommand<Guid>;
