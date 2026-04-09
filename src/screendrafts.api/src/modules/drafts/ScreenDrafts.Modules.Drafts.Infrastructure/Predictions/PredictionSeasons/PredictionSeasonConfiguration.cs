namespace ScreenDrafts.Modules.Drafts.Infrastructure.Predictions.PredictionSeasons;

internal sealed class PredictionSeasonConfiguration : IEntityTypeConfiguration<PredictionSeason>
{
  public void Configure(EntityTypeBuilder<PredictionSeason> builder)
  {
    builder.ToTable(Tables.PredictionSeasons);

    builder.HasKey(t => t.Id);

    builder.Property(s => s.Id)
      .ValueGeneratedNever()
      .HasConversion(IdConverters.PredictionSeasonIdConverter);

    builder.Property(s => s.Number)
      .IsRequired();

    builder.HasIndex(s => s.Number)
      .IsUnique();

    builder.Property(s => s.PublicId)
      .IsRequired();

    builder.HasIndex(s => s.PublicId)
      .IsUnique();

    builder.Property(s => s.StartsOn)
      .IsRequired();

    builder.Property(s => s.EndsOn);

    builder.Property(s => s.TargetPoints)
      .IsRequired()
      .HasDefaultValue(100);

    builder.Property(s => s.IsClosed)
      .IsRequired()
      .HasDefaultValue(false);

    builder.HasMany(s => s.Standings)
      .WithOne(st => st.Season)
      .HasForeignKey(s => s.SeasonId)
      .OnDelete(DeleteBehavior.Cascade);

    builder.HasMany(s => s.Carryovers)
      .WithOne(c => c.Season)
      .HasForeignKey(c => c.SeasonId)
      .OnDelete(DeleteBehavior.Cascade);

    builder.HasMany(s => s.Sets)
      .WithOne(set => set.Season)
      .HasForeignKey(set => set.SeasonId)
      .OnDelete(DeleteBehavior.Restrict);
  }
}
