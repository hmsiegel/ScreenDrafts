
namespace ScreenDrafts.Modules.Drafts.Application.Drafts.Queries.ListCategories;

public sealed record CategoryResponse(
    Guid Id,
    string Name,
    string? Description)
{
  public CategoryResponse()
      : this(
          Guid.Empty,
          string.Empty,
          string.Empty)
  {
  }
}
