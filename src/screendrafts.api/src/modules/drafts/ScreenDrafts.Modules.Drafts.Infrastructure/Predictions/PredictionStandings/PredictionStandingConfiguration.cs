namespace ScreenDrafts.Modules.Drafts.Infrastructure.Predictions.PredictionStandings;

internal sealed class PredictionStandingConfiguration : IEntityTypeConfiguration<PredictionStanding>
{
  public void Configure(EntityTypeBuilder<PredictionStanding> builder)
  {
    builder.ToTable(Tables.PredictionStandings);

    builder.HasKey(s => s.Id);

    builder.Property(s => s.Id)
      .ValueGeneratedNever()
      .HasConversion(IdConverters.PredictionStandingIdConverter);

    builder.Property(s => s.SeasonId)
      .IsRequired()
      .HasConversion(IdConverters.PredictionSeasonIdConverter);

    builder.Property(s => s.ContestantId)
      .IsRequired()
      .HasConversion(IdConverters.ContestantIdConverter);

    // One standing row per contestant per season.
    builder.HasIndex(s => new { s.ContestantId, s.SeasonId })
      .IsUnique()
      .HasDatabaseName("ux_prediction_standings_contestant_season");

    builder.Property(s => s.Points)
      .IsRequired();

    builder.Property(s => s.FirstCrossedTargetAtUtc);

    builder.HasOne(s => s.Season)
      .WithMany(season => season.Standings)
      .HasForeignKey(s => s.SeasonId)
      .OnDelete(DeleteBehavior.Cascade);

    builder.HasOne(s => s.Contestant)
      .WithMany()
      .HasForeignKey(s => s.ContestantId)
      .OnDelete(DeleteBehavior.Restrict);
  }
}
