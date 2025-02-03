namespace ScreenDrafts.Modules.Reporting.Infrastructure.Database;

public sealed class ReportingDbContext(DbContextOptions<ReportingDbContext> options)
  : DbContext(options), IUnitOfWork
{

  protected override void OnModelCreating(ModelBuilder modelBuilder)
  {
    ArgumentNullException.ThrowIfNull(modelBuilder);

    modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    modelBuilder.ApplyConfigurationsFromAssembly(typeof(InfrastructureConfiguration).Assembly);

    modelBuilder.HasDefaultSchema(Schemas.Reporting);
  }
}
