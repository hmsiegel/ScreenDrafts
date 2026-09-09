namespace ScreenDrafts.Modules.GuestDrafts.Infrastructure.Database;

public sealed class GuestDraftsDbContext(DbContextOptions<GuestDraftsDbContext> options)
  : DbContext(options),
    IUnitOfWork
{
  internal DbSet<Draft> Drafts { get; set; }
  internal DbSet<DraftParticipant> DraftParticipants { get; set; }
  internal DbSet<GameBoard> GameBoards { get; set; }
  internal DbSet<DraftPosition> DraftPositions { get; set; }
  internal DbSet<Pick> Picks { get; set; }
  internal DbSet<Veto> Vetoes { get; set; }
  internal DbSet<VetoOverride> VetoOverrides { get; set; }
  internal DbSet<CommissionerOverride> CommissionerOverrides { get; set; }
  internal DbSet<Drafter> Drafters { get; set; }
  internal DbSet<DrafterTeam> DrafterTeams { get; set; }
  internal DbSet<Movie> Movies { get; set; }

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
