namespace ScreenDrafts.Modules.Movies.Infrastructure.Database;

public abstract class MoviesRepositoryBase<TEntity>(MoviesDbContext dbContext)
  where TEntity : class
{
    private readonly MoviesDbContext _dbContext = dbContext;

    protected MoviesDbContext DbContext => _dbContext;

    public void Attach(TEntity entity)
    {
        if (_dbContext.Entry(entity).State == EntityState.Detached)
        {
            _dbContext.Set<TEntity>().Attach(entity);
        }
    }
}
