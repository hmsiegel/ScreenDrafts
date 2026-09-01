namespace ScreenDrafts.Modules.GuestDrafts.Infrastructure.Database;

public sealed class GuestDraftsDbContext(DbContextOptions<GuestDraftsDbContext> options)
  : DbContext(options),
    IUnitOfWork
{
  internal DbSet<GuestDraft> GuestDrafts { get; set; }
  internal DbSet<GuestDraftParticipant> GuestDraftParticipants { get; set; }
  internal DbSet<GuestDraftGameBoard> GuestDraftGameBoards { get; set; }
  internal DbSet<GuestDraftPosition> GuestDraftPositions { get; set; }
  internal DbSet<GuestDraftPick> GuestDraftPicks { get; set; }
  internal DbSet<GuestDraftVeto> GuestDraftVetoes { get; set; }
  internal DbSet<GuestDraftVetoOverride> GuestDraftVetoOverrides { get; set; }
  internal DbSet<GuestDraftCommissionerOverride> GuestDraftCommissionerOverrides { get; set; }

  protected override void OnModelCreating(ModelBuilder modelBuilder)
  {
    ArgumentNullException.ThrowIfNull(modelBuilder);

    modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    modelBuilder.ApplyConfigurationsFromAssembly(typeof(InfrastructureConfiguration).Assembly);

    modelBuilder.HasDefaultSchema(Schemas.GuestDrafts);
  }

  protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
  {
    ArgumentNullException.ThrowIfNull(configurationBuilder);

    configurationBuilder.ConfigureSmartEnum();
  }
}
