namespace ScreenDrafts.Modules.Drafts.Infrastructure.Predictions.DraftPredictionSets;

internal sealed class DraftPredictionSetConfiguration : IEntityTypeConfiguration<DraftPredictionSet>
{
  public void Configure(EntityTypeBuilder<DraftPredictionSet> builder)
  {
    builder.ToTable(Tables.DraftPredictionSets);

    builder.HasKey(s => s.Id);

    builder.Property(s => s.Id)
      .ValueGeneratedNever()
      .HasConversion(IdConverters.DraftPredictionSetIdConverter);

    builder.Property(s => s.PublicId)
      .IsRequired()
      .HasMaxLength(PublicIdPrefixes.MaxPublicIdLength);

    builder.HasIndex(s => s.PublicId)
      .IsUnique();

    builder.Property(s => s.SeasonId)
      .IsRequired()
      .HasConversion(IdConverters.PredictionSeasonIdConverter);

    builder.Property(s => s.DraftPartId)
      .IsRequired()
      .HasConversion(IdConverters.DraftPartIdConverter);

    builder.Property(s => s.ContestantId)
      .IsRequired()
      .HasConversion(IdConverters.ContestantIdConverter);

    // One set per contestant per draft part.
    builder.HasIndex(s => new { s.ContestantId, s.DraftPartId })
      .IsUnique()
      .HasDatabaseName("ux_draft_prediction_sets_contestant_part");

    builder.Property(s => s.SubmittedByPersonId)
      .HasConversion(IdConverters.NullablePersonIdConverter);

    builder.Property(s => s.SubmittedAtUtc)
      .IsRequired();

    builder.Property(s => s.SourceKind)
      .IsRequired()
      .HasConversion(EnumConverters.PredictionSourceKindConverter);

    builder.Property(s => s.LockedAtUtc);

    // RulesSnapshot is an owned type (value object record).
    builder.OwnsOne(s => s.RulesSnapshot, snapshot =>
    {
      snapshot.Property(r => r.Mode)
        .HasColumnName("snapshot_mode")
        .HasConversion(EnumConverters.PredictionModeConverter);

      snapshot.Property(r => r.RequiredCount)
        .HasColumnName("snapshot_required_count");

      snapshot.Property(r => r.TopN)
        .HasColumnName("snapshot_top_n");
    });

    builder.HasMany(s => s.Entries)
      .WithOne(e => e.PredictionSet)
      .HasForeignKey(e => e.SetId)
      .OnDelete(DeleteBehavior.Cascade);

    builder.HasMany(s => s.Surrogates)
      .WithOne(sa => sa.PrimarySet)
      .HasForeignKey(sa => sa.PrimarySetId)
      .OnDelete(DeleteBehavior.Restrict);

    builder.HasOne(s => s.Season)
      .WithMany(season => season.Sets)
      .HasForeignKey(s => s.SeasonId)
      .OnDelete(DeleteBehavior.Restrict);

    builder.HasOne(s => s.DraftPart)
      .WithMany()
      .HasForeignKey(s => s.DraftPartId)
      .OnDelete(DeleteBehavior.Restrict);

    builder.HasOne(s => s.Contestant)
      .WithMany()
      .HasForeignKey(s => s.ContestantId)
      .OnDelete(DeleteBehavior.Restrict);
  }
}
