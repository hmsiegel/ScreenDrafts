namespace ScreenDrafts.Modules.Drafts.Infrastructure.Predictions;

internal sealed class PredictionEntryConfiguration : IEntityTypeConfiguration<PredictionEntry>
{
  public void Configure(EntityTypeBuilder<PredictionEntry> builder)
  {
    builder.ToTable(Tables.PredictionEntries);

    builder.HasKey(e => e.Id);

    builder.Property(e => e.Id)
      .ValueGeneratedNever()
      .HasConversion(IdConverters.PredictionEntryIdConverter);

    builder.Property(e => e.SetId)
      .IsRequired()
      .HasConversion(IdConverters.DraftPredictionSetIdConverter);

    builder.Property(e => e.MediaPublicId)
      .IsRequired()
      .HasMaxLength(PublicIdPrefixes.MaxPublicIdLength);

    builder.Property(e => e.MediaTitle)
      .IsRequired()
      .HasMaxLength(500);

    builder.Property(e => e.OrderIndex);

    builder.Property(e => e.Notes)
      .HasMaxLength(1000);

    builder.HasOne(e => e.PredictionSet)
      .WithMany(s => s.Entries)
      .HasForeignKey(e => e.SetId)
      .OnDelete(DeleteBehavior.Cascade);
  }
}
