namespace ScreenDrafts.Modules.Drafts.Application.Drafts.Commands.AddCategory;

public sealed record CreateCategoryCommand(string Name, string? Description) : ICommand<Guid>;
