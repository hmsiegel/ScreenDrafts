namespace ScreenDrafts.Modules.Drafts.Infrastructure.Predictions.PredictionResults;

internal sealed class PredictionResultConfiguration : IEntityTypeConfiguration<PredictionResult>
{
  public void Configure(EntityTypeBuilder<PredictionResult> builder)
  {
    builder.ToTable(Tables.PredictionResults);

    builder.HasKey(r => r.Id);

    builder.Property(r => r.Id)
      .ValueGeneratedNever()
      .HasConversion(IdConverters.PredictionResultIdConverter);

    builder.Property(r => r.SetId)
      .IsRequired()
      .HasConversion(IdConverters.DraftPredictionSetIdConverter);

    builder.HasIndex(r => r.SetId)
      .IsUnique()
      .HasDatabaseName("ux_prediction_results_set_id");

    builder.Property(r => r.CorrectCount)
      .IsRequired();

    builder.Property(r => r.ShootTheMoon)
      .IsRequired()
      .HasDefaultValue(false);

    builder.Property(r => r.PointsAwarded)
      .IsRequired();

    builder.Property(r => r.ScoredAtUtc)
      .IsRequired();

    builder.HasOne(r => r.PredictionSet)
      .WithMany()
      .HasForeignKey(r => r.SetId)
      .OnDelete(DeleteBehavior.Restrict);
  }
}
