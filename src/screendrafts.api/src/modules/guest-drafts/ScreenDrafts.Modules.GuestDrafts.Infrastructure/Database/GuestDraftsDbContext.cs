using ScreenDrafts.Modules.GuestDrafts.Domain.Drafters;
using ScreenDrafts.Modules.GuestDrafts.Domain.DrafterTeams;
using ScreenDrafts.Modules.GuestDrafts.Domain.Drafts;
using ScreenDrafts.Modules.GuestDrafts.Domain.Drafts.Entities;

namespace ScreenDrafts.Modules.GuestDrafts.Infrastructure.Database;

public sealed class GuestDraftsDbContext(DbContextOptions<GuestDraftsDbContext> options)
  : DbContext(options),
    IUnitOfWork
{
  internal DbSet<Draft> GuestDrafts { get; set; }
  internal DbSet<DraftParticipant> GuestDraftParticipants { get; set; }
  internal DbSet<GameBoard> GuestDraftGameBoards { get; set; }
  internal DbSet<DraftPosition> GuestDraftPositions { get; set; }
  internal DbSet<Pick> GuestDraftPicks { get; set; }
  internal DbSet<Veto> GuestDraftVetoes { get; set; }
  internal DbSet<VetoOverride> GuestDraftVetoOverrides { get; set; }
  internal DbSet<CommissionerOverride> GuestDraftCommissionerOverrides { get; set; }
  internal DbSet<Drafter> GuestDrafters { get; set; }
  internal DbSet<DrafterTeam> GuestDrafterTeams { get; set; }
  internal DbSet<Movie> GuestDraftMovies { get; set; }

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
