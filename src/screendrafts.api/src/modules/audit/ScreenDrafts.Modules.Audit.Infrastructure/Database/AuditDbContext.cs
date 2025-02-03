namespace ScreenDrafts.Modules.Audit.Infrastructure.Database;

public sealed class AuditDbContext(DbContextOptions<AuditDbContext> options)
  : DbContext(options), IUnitOfWork
{

  protected override void OnModelCreating(ModelBuilder modelBuilder)
  {
    ArgumentNullException.ThrowIfNull(modelBuilder);

    modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    modelBuilder.ApplyConfigurationsFromAssembly(typeof(InfrastructureConfiguration).Assembly);

    modelBuilder.HasDefaultSchema(Schemas.Audit);
  }
}
