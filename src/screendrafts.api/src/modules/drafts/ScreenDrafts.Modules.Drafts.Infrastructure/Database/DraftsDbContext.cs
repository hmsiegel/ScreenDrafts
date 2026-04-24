using ScreenDrafts.Modules.Drafts.Domain.Predictions;
using ScreenDrafts.Modules.Drafts.Domain.Predictions.Entities;

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
  internal DbSet<DraftBoard> DraftBoards { get; set; }
  internal DbSet<DraftPool> DraftPools { get; set; }
  internal DbSet<CandidateListEntry> CandidateListEntries { get; set; }
  internal DbSet<PredictionSeason> PredictionSeasons { get; set; } 
  internal DbSet<PredictionContestant> PredictionContestants { get; set; } 
  internal DbSet<DraftPartPredictionRule> DraftPartPredictionRules { get; set; } 
  internal DbSet<DraftPredictionSet> DraftPredictionSets { get; set; } 
  internal DbSet<PredictionEntry> PredictionEntries { get; set; } 
  internal DbSet<SurrogateAssignment> SurrogateAssignments { get; set; } 
  internal DbSet<PredictionResult> PredictionResults { get; set; } 
  internal DbSet<PredictionStanding> PredictionStandings { get; set; } 
  internal DbSet<PredictionCarryover> PredictionCarryovers { get; set; } 
  internal DbSet<DraftPartRecording> DraftPartRecordings { get; set; }

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
