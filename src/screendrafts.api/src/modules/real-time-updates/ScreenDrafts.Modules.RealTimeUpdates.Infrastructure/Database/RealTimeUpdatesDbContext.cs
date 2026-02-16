using ScreenDrafts.Modules.RealTimeUpdates.Domain.Abstractions.Data;

namespace ScreenDrafts.Modules.RealTimeUpdates.Infrastructure.Database;

public sealed class RealTimeUpdatesDbContext(DbContextOptions<RealTimeUpdatesDbContext> options)
  : DbContext(options), IUnitOfWork
{

  protected override void OnModelCreating(ModelBuilder modelBuilder)
  {
    ArgumentNullException.ThrowIfNull(modelBuilder);

    modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    modelBuilder.ApplyConfigurationsFromAssembly(typeof(InfrastructureConfiguration).Assembly);

    modelBuilder.HasDefaultSchema(Schemas.RealTimeUpdates);
  }
}
