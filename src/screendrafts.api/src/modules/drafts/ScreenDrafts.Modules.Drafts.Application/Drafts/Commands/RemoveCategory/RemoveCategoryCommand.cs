namespace ScreenDrafts.Modules.Drafts.Application.Drafts.Commands.RemoveCategory;

public sealed record RemoveCategoryCommand(Guid CategoryId) : ICommand;
