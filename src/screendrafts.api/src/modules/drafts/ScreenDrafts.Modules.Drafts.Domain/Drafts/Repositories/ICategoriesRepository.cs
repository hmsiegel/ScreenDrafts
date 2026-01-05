namespace ScreenDrafts.Modules.Drafts.Domain.Drafts.Repositories;
public interface ICategoriesRepository : IRepository
{
  void AddCategory(Category category);

  void RemoveCategory(Category category);

  Task<Category?> GetByIdAsync(CategoryId categoryId, CancellationToken cancellationToken);

  Task<bool> ExistsAsync(CategoryId categoryId, CancellationToken cancellationToken);

  Task<bool> IsCategoryInUseAsync(CategoryId categoryId, CancellationToken cancellationToken);
}
