namespace ScreenDrafts.Modules.Drafts.Domain.Drafts.Repositories;
public interface ICategoriesRepository : IRepository<Category, CategoryId>
{

  Task<Category?> GetByPublicIdAsync(string publicId, CancellationToken cancellationToken);

  Task<bool> ExistsByNameAsync(string name, CancellationToken cancellationToken);

  Task<bool> IsCategoryInUseAsync(CategoryId categoryId, CancellationToken cancellationToken);
}
