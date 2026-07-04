namespace ScreenDrafts.Modules.GuestDrafts.Infrastructure.Database;

public sealed class GuestDraftsDbContext(DbContextOptions<GuestDraftsDbContext> options)
  : DbContext(options),
    IUnitOfWork
{
  protected override void OnModelCreating(ModelBuilder modelBuilder)
  {
    ArgumentNullException.ThrowIfNull(modelBuilder);

    modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    modelBuilder.ApplyConfigurationsFromAssembly(typeof(InfrastructureConfiguration).Assembly);

    modelBuilder.HasDefaultSchema(Schemas.Drafts);
  }

  protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
  {
    ArgumentNullException.ThrowIfNull(configurationBuilder);

    configurationBuilder.ConfigureSmartEnum();
  }
}
