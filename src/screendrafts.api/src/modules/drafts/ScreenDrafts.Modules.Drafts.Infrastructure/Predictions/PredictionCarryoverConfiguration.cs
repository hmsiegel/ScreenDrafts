namespace ScreenDrafts.Modules.Drafts.Infrastructure.Predictions;

internal sealed class PredictionCarryoverConfiguration : IEntityTypeConfiguration<PredictionCarryover>
{
  public void Configure(EntityTypeBuilder<PredictionCarryover> builder)
  {
    builder.ToTable(Tables.PredictionCarryovers);

    builder.HasKey(c => c.Id);

    builder.Property(c => c.Id)
      .ValueGeneratedNever()
      .HasConversion(IdConverters.PredictionCarryoverIdConverter);

    builder.Property(c => c.SeasonId)
      .IsRequired()
      .HasConversion(IdConverters.PredictionSeasonIdConverter);

    builder.Property(c => c.ContestantId)
      .IsRequired()
      .HasConversion(IdConverters.ContestantIdConverter);

    builder.Property(c => c.Points)
      .IsRequired();

    builder.Property(c => c.Kind)
      .IsRequired()
      .HasConversion(k => k.Value, value => CarryoverKind.FromValue(value));

    builder.Property(c => c.Reason)
      .HasMaxLength(500);

    builder.HasOne(c => c.Season)
      .WithMany(s => s.Carryovers)
      .HasForeignKey(c => c.SeasonId)
      .OnDelete(DeleteBehavior.Cascade);

    builder.HasOne(c => c.Contestant)
      .WithMany()
      .HasForeignKey(c => c.ContestantId)
      .OnDelete(DeleteBehavior.Restrict);
  }
}
