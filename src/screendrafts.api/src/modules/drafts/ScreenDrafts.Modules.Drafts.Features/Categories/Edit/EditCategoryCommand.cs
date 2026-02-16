namespace ScreenDrafts.Modules.Drafts.Features.Categories.Edit;

internal sealed record EditCategoryCommand : ICommand
{
  public string PublicId { get; init; } = string.Empty;
  public string? Name { get; init; }
  public string? Description { get; init; }
}

