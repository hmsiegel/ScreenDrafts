namespace ScreenDrafts.Modules.Drafts.Domain.Categories;
public interface ICategoryRepository : IRepository<Category, CategoryId>
{
  Task<Category?> GetByPublicIdAsync(string publicId, CancellationToken cancellationToken);
  Task<bool> ExistsByNameAsync(string name, CancellationToken cancellationToken);
  Task<bool> IsCategoryInUseAsync(CategoryId categoryId, CancellationToken cancellationToken);
  Task<bool> AllExistByPublicIdsAsync(IReadOnlyList<string> publicCategoryIds, CancellationToken cancellationToken);
  Task<IReadOnlyList<Category>> GetByPublicIdsAsync(IReadOnlyCollection<string> publicCategoryIds, CancellationToken cancellationToken);
}
