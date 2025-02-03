namespace ScreenDrafts.Modules.Administration.Infrastructure.Database;

public sealed class AdministrationDbContext(DbContextOptions<AdministrationDbContext> options)
  : DbContext(options), IUnitOfWork
{

  protected override void OnModelCreating(ModelBuilder modelBuilder)
  {
    ArgumentNullException.ThrowIfNull(modelBuilder);

    modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    modelBuilder.ApplyConfigurationsFromAssembly(typeof(InfrastructureConfiguration).Assembly);

    modelBuilder.HasDefaultSchema(Schemas.Administration);
  }
}
