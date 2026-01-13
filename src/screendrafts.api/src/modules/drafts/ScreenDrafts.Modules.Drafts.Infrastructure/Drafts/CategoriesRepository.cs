
namespace ScreenDrafts.Modules.Drafts.Infrastructure.Drafts;

internal sealed class CategoriesRepository(DraftsDbContext dbContext) : ICategoriesRepository
{
  private readonly DraftsDbContext _dbContext = dbContext;

  public void Add(Category category)
  {
    _dbContext.Categories.Add(category);
  }

  public Task<bool> ExistsAsync(CategoryId categoryId, CancellationToken cancellationToken)
  {
    return _dbContext.Categories
      .AnyAsync(c => c.Id == categoryId, cancellationToken);
  }

  public async Task<Category?> GetByIdAsync(CategoryId categoryId, CancellationToken cancellationToken)
  {
    var category = await _dbContext.Categories
      .FirstOrDefaultAsync(c => c.Id == categoryId, cancellationToken);

    return category;
  }

  public async Task<bool> IsCategoryInUseAsync(CategoryId categoryId, CancellationToken cancellationToken)
  {
    var isInUse = await _dbContext.Drafts
      .AnyAsync(d => d.DraftCategories.Any(c => c.CategoryId == categoryId), cancellationToken);

    return isInUse;
  }

  public void RemoveCategory(Category category)
  {
    _dbContext.Categories.Remove(category);
  }
}
