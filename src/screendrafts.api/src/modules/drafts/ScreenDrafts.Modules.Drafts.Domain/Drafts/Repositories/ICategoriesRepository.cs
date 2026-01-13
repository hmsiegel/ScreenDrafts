namespace ScreenDrafts.Modules.Drafts.Domain.Drafts.Repositories;
public interface ICategoriesRepository : IRepository
{
  void Add(Category category);

  void Remove(Category category);

  void Update(Category category);

  Task<Category?> GetByIdAsync(CategoryId categoryId, CancellationToken cancellationToken);

  Task<Category?> GetByPublicIdAsync(string publicId, CancellationToken cancellationToken);

  Task<bool> ExistsAsync(CategoryId categoryId, CancellationToken cancellationToken);

  Task<bool> ExistsAsync(string name, CancellationToken cancellationToken);

  Task<bool> IsCategoryInUseAsync(CategoryId categoryId, CancellationToken cancellationToken);
}
