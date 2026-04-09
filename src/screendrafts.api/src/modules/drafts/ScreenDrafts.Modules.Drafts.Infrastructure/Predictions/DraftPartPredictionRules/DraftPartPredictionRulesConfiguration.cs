namespace ScreenDrafts.Modules.Drafts.Infrastructure.Predictions.DraftPartPredictionRules;

internal sealed class DraftPartPredictionRulesConfiguration : IEntityTypeConfiguration<DraftPartPredictionRule>
{
  public void Configure(EntityTypeBuilder<DraftPartPredictionRule> builder)
  {
    builder.ToTable(Tables.DraftPartPredictionRules);
    
    builder.HasKey(t => t.Id);

    builder.Property(s => s.Id)
      .ValueGeneratedNever()
      .HasConversion(IdConverters.DraftPartPredictionRulesIdConverter);

    builder.Property(s => s.DraftPartId)
      .ValueGeneratedNever()
      .HasConversion(IdConverters.DraftPartIdConverter);

    builder.HasIndex(r => r.DraftPartId)
      .IsUnique();

    builder.Property(x => x.PublicId)
      .IsRequired()
      .HasMaxLength(PublicIdPrefixes.MaxPublicIdLength);

    builder.HasIndex(r => r.PublicId)
      .IsUnique();

    builder.Property(r => r.PredictionMode)
      .IsRequired()
      .HasConversion(EnumConverters.PredictionModeConverter);

    builder.Property(r => r.RequiredCount)
      .IsRequired()
      .HasDefaultValue(7);

    builder.Property(r => r.TopN);

    builder.Property(r => r.DeadlineUtc);
    
    builder.Property(r => r.CreatedOnUtc)
      .IsRequired();

    builder.Property(r => r.UpdatedOnUtc);

    builder.HasOne(r => r.DraftPart)
      .WithOne()
      .HasForeignKey<DraftPartPredictionRule>(r => r.DraftPartId)
      .OnDelete(DeleteBehavior.Restrict);
  }
}
