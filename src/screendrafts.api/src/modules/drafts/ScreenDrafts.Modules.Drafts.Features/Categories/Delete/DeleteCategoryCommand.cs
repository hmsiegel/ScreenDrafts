namespace ScreenDrafts.Modules.Drafts.Features.Categories.Delete;

internal sealed record DeleteCategoryCommand : ICommand
{
  public required string PublicId { get; init; }
}
