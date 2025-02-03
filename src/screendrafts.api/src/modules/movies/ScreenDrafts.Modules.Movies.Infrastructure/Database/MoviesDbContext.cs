namespace ScreenDrafts.Modules.Movies.Infrastructure.Database;

public sealed class MoviesDbContext(DbContextOptions<MoviesDbContext> options)
  : DbContext(options), IUnitOfWork
{

  protected override void OnModelCreating(ModelBuilder modelBuilder)
  {
    ArgumentNullException.ThrowIfNull(modelBuilder);

    modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    modelBuilder.ApplyConfigurationsFromAssembly(typeof(InfrastructureConfiguration).Assembly);

    modelBuilder.HasDefaultSchema(Schemas.Movies);
  }
}
