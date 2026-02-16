namespace ScreenDrafts.Modules.Drafts.Infrastructure.Database;

public sealed class DraftsDbContext(DbContextOptions<DraftsDbContext> options)
  : DbContext(options), IUnitOfWork
{
  internal DbSet<Draft> Drafts { get; set; }

  internal DbSet<Person> People { get; set; }

  internal DbSet<Drafter> Drafters { get; set; }

  internal DbSet<Host> Hosts { get; set; }

  internal DbSet<TriviaResult> TriviaResults { get; set; }

  internal DbSet<Pick> Picks { get; set; }

  internal DbSet<Veto> Vetoes { get; set; }

  internal DbSet<VetoOverride> VetoOverrides { get; set; }

  internal DbSet<Movie> Movies { get; set; }

  internal DbSet<GameBoard> GameBoards { get; set; }

  internal DbSet<DraftPosition> DraftPositions { get; set; }

  internal DbSet<CommissionerOverride> CommissionerOverrides { get; set; }

  internal DbSet<DrafterTeam> DrafterTeams { get; set; }

  internal DbSet<Category> Categories { get; set; }

  internal DbSet<Series> Series { get; set; }

  internal DbSet<Campaign> Campaigns { get; set; }

  internal DbSet<DraftPart> DraftParts { get; set; }

  internal DbSet<DraftRelease> DraftReleases { get; set; }

  internal DbSet<DraftChannelRelease> DraftChannelReleases { get; set; }

  internal DbSet<DraftPartParticipant> DraftPartParticipants { get; set; }

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
