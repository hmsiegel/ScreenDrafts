﻿namespace ScreenDrafts.Modules.Integrations.Infrastructure.Database;

public sealed class IntegrationsDbContext(DbContextOptions<IntegrationsDbContext> options)
  : DbContext(options), IUnitOfWork
{

  protected override void OnModelCreating(ModelBuilder modelBuilder)
  {
    ArgumentNullException.ThrowIfNull(modelBuilder);

    modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    modelBuilder.ApplyConfigurationsFromAssembly(typeof(InfrastructureConfiguration).Assembly);

    modelBuilder.HasDefaultSchema(Schemas.Integrations);
  }
}
