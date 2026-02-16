using ScreenDrafts.Modules.Drafts.Domain.Categories;

namespace ScreenDrafts.Modules.Drafts.Domain.Drafts.Entities;

public sealed class DraftCategory
{
  private DraftCategory(
    Draft draft,
    Category category)
  {
    DraftId = draft.Id;
    Draft = draft;
    CategoryId = category.Id;
    Category = category;
  }

  private DraftCategory()
  {
    // EF Core
  }

  public DraftId DraftId { get; private set; } = default!;
  public Draft Draft { get; private set; } = default!;

  public CategoryId CategoryId { get; private set; } = default!;
  public Category Category { get; private set; } = default!;

  public static DraftCategory Create(Draft draft, Category category)
  {
    ArgumentNullException.ThrowIfNull(draft);
    ArgumentNullException.ThrowIfNull(category);
    return new DraftCategory(draft, category);
  }
}
