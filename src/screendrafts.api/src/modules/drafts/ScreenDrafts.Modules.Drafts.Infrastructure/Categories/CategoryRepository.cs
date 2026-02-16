namespace ScreenDrafts.Modules.Drafts.Infrastructure.Categories;

internal sealed class CategoryRepository(DraftsDbContext dbContext) : ICategoryRepository
{
  private readonly DraftsDbContext _dbContext = dbContext;

  public void Add(Category category)
  {
    _dbContext.Categories.Add(category);
  }

  public void Update(Category category)
  {
    _dbContext.Categories.Update(category);
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

  public void Delete(Category category)
  {
    _dbContext.Categories.Remove(category);
  }

  public Task<Category?> GetByPublicIdAsync(string publicId, CancellationToken cancellationToken)
  {
    return _dbContext.Categories.FirstOrDefaultAsync(c => c.PublicId == publicId, cancellationToken);
  }

  public Task<bool> ExistsByNameAsync(string name, CancellationToken cancellationToken)
  {
    return _dbContext.Categories.AnyAsync(c => c.Name == name && !c.IsDeleted, cancellationToken);
  }

  public async Task<bool> AllExistByPublicIdsAsync(IReadOnlyList<string> publicCategoryIds, CancellationToken cancellationToken)
  {
    if (publicCategoryIds.Count == 0)
    {
      return true;
    }

    var count = await _dbContext.Categories
      .CountAsync(c => publicCategoryIds.Contains(c.PublicId) && !c.IsDeleted, cancellationToken);

    return count == publicCategoryIds.Count;
  }

  public async Task<IReadOnlyList<Category>> GetByPublicIdsAsync(IReadOnlyCollection<string> publicCategoryIds, CancellationToken cancellationToken)
  {
    if (publicCategoryIds.Count == 0)
    {
      return [];
    }

    return await _dbContext.Categories
      .Where(c => publicCategoryIds.Contains(c.PublicId) && !c.IsDeleted)
      .ToListAsync(cancellationToken);
  }
}
