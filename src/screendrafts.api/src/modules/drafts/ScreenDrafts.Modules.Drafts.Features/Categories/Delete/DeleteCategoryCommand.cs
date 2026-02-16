namespace ScreenDrafts.Modules.Drafts.Features.Categories.Delete;

internal sealed record DeleteCategoryCommand(string PublicId) : ICommand;

