namespace ScreenDrafts.Modules.Drafts.Application.Drafters.Commands.CreateDrafter;
public sealed record CreateDrafterCommand(Guid PersonId) : ICommand<Guid>;
