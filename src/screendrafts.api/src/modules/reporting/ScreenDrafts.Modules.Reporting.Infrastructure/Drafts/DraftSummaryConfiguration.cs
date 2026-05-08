namespace ScreenDrafts.Modules.Reporting.Infrastructure.Drafts;

internal sealed class DraftSummaryConfiguration : IEntityTypeConfiguration<DraftSummary>
{
  public void Configure(EntityTypeBuilder<DraftSummary> builder)
  {
    builder.ToTable(Tables.DraftSummaries);

    builder.HasKey(x => x.Id);

    builder
      .Property(x => x.Id)
      .ValueGeneratedNever()
      .HasConversion(IdConverters.DraftSummaryIdConverter);

    builder.Property(x => x.DraftId).IsRequired();

    builder.HasIndex(x => x.DraftId).HasDatabaseName("ix_draft_summaries_draft_id");

    builder.Property(x => x.DraftPublicId).IsRequired();

    builder.HasIndex(x => x.DraftPublicId).HasDatabaseName("ix_draft_summaries_draft_public_id");

    builder.Property(x => x.DraftPartPublicId).IsRequired();

    builder.Property(x => x.Title).IsRequired();

    builder.Property(x => x.DraftType).IsRequired();

    builder.Property(x => x.PartIndex);

    builder.Property(x => x.TotalParts);

    builder.Property(x => x.IsPatreon);

    builder.Property(x => x.EpisodeNumber);

    builder.Property(x => x.IsComplete);

    builder.Property(x => x.CompletedAtUtc);

    builder.Property(x => x.CreatedAtUtc);

    builder
      .HasIndex(x => new { x.DraftId, x.DraftPartPublicId })
      .IsUnique()
      .HasDatabaseName("ux_draft_summaries_draft_id_part_public_id");
  }
}
