namespace ScreenDrafts.Modules.Reporting.Infrastructure.Database;

public sealed class ReportingDbContext(DbContextOptions<ReportingDbContext> options)
  : DbContext(options), IUnitOfWork
{
  internal DbSet<DrafterHonorificEntity> DrafterHonorifics { get; set; } = default!;
  internal DbSet<DrafterHonorificHistory> DraftersHonorificHistory { get; set; } = default!;
  internal DbSet<DrafterCanonicalAppearance> DrafterCanonicalAppearances { get; set; } = default!;
  internal DbSet<MovieHonorificEntity> MovieHonorifics { get; set; } = default!;
  internal DbSet<MovieHonorificHistory> MoviesHonorificHistory { get; set; } = default!;
  internal DbSet<MovieCanonicalPick> MovieCanonicalPicks { get; set; } = default!;

  protected override void OnModelCreating(ModelBuilder modelBuilder)
  {
    ArgumentNullException.ThrowIfNull(modelBuilder);

    modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    modelBuilder.ApplyConfigurationsFromAssembly(typeof(InfrastructureConfiguration).Assembly);

    modelBuilder.HasDefaultSchema(Schemas.Reporting);
  }

  protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
  {
    ArgumentNullException.ThrowIfNull(configurationBuilder);

    configurationBuilder.ConfigureSmartEnum();
  }
}
