namespace ScreenDrafts.Modules.Drafts.Infrastructure.Predictions;

internal sealed class DraftPartPredictorsConfiguration
  : IEntityTypeConfiguration<DraftPartPredictor>
{
  public void Configure(EntityTypeBuilder<DraftPartPredictor> builder)
  {
    builder.ToTable(Tables.DraftPartPredictors);

    builder.HasKey(x => x.Id);

    builder.Property(x => x.Id).HasConversion(IdConverters.DraftPartPredictorIdConverter);

    builder.Property(x => x.PublicId).HasMaxLength(PublicIdPrefixes.MaxPublicIdLength).IsRequired();

    builder
      .Property(x => x.DraftPartId)
      .ValueGeneratedNever()
      .HasConversion(IdConverters.DraftPartIdConverter);

    builder
      .Property(x => x.ContestantId)
      .ValueGeneratedNever()
      .HasConversion(IdConverters.ContestantIdConverter);

    builder
      .Property(x => x.AllowedSubmitterPersonId)
      .HasConversion(IdConverters.NullablePersonIdConverter);

    builder.HasIndex(x => new { x.DraftPartId, x.ContestantId }).IsUnique();

    builder.HasIndex(x => x.PublicId).IsUnique();

    builder
      .HasOne(t => t.Contestant)
      .WithMany()
      .HasForeignKey(t => t.ContestantId)
      .OnDelete(DeleteBehavior.Restrict);
  }
}
