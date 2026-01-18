namespace ScreenDrafts.Modules.Drafts.Features.Categories;

internal sealed record CategoryResponse
{
  public required string PublicId { get; init; }
  public required string Name { get; init; }
  public required string Description { get; init; }
  public bool IsDeleted { get; init; }
}
