namespace ScreenDrafts.Common.Domain;

public interface IRepository<T, TId> : IRepository
{
  void Add(T entity);
  void Update(T entity);
  void Delete(T entity);
  Task<T?> GetByIdAsync(TId id, CancellationToken cancellationToken);
  Task<bool> ExistsAsync(TId id, CancellationToken cancellationToken);
}

public interface IRepository;

public interface IRepository<in T> : IRepository
{
  void Add(T entity);
  void Update(T entity);
  void Delete(T entity);
}
