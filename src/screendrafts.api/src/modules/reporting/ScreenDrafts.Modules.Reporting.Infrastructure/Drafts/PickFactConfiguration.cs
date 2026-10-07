namespace ScreenDrafts.Modules.Reporting.Infrastructure.Drafts;

internal sealed class PickFactConfiguration : IEntityTypeConfiguration<PickFact>
{
  public void Configure(EntityTypeBuilder<PickFact> builder)
  {
    builder.ToTable(Tables.PickFacts);

    builder.HasKey(x => x.Id);
    builder.Property(x => x.Id).ValueGeneratedNever();

    builder.Property(x => x.DraftId);
    builder.Property(x => x.DraftPublicId);
    builder.Property(x => x.DraftPartPublicId);
    builder.Property(x => x.PartIndex);
    builder.Property(x => x.DraftTitle);
    builder.Property(x => x.DraftType);
    builder.Property(x => x.SeriesName);
    builder.Property(x => x.CanonicalPolicy);
    builder.Property(x => x.SubDraftIndex);
    builder.Property(x => x.Position);
    builder.Property(x => x.PlayOrder);
    builder.Property(x => x.MediaPublicId);
    builder.Property(x => x.MediaTitle);
    builder.Property(x => x.PlayedByKind);
    builder.Property(x => x.PlayedByIdValue);
    builder.Property(x => x.PlayedByPublicId);
    builder.Property(x => x.PlayedByName);
    builder.Property(x => x.VetoCount);
    builder.Property(x => x.WasVetoed);
    builder.Property(x => x.WasVetoOverridden);
    builder.Property(x => x.WasCommissionerOverridden);
    builder.Property(x => x.RecordedAtUtc);

    builder
      .HasIndex(x => x.DraftPartPublicId)
      .HasDatabaseName("ix_pick_facts_draft_part_public_id");
    builder.HasIndex(x => x.DraftId).HasDatabaseName("ix_pick_facts_draft_id");
    builder.HasIndex(x => x.MediaPublicId).HasDatabaseName("ix_pick_facts_media_public_id");
    builder
      .HasIndex(x => new { x.PlayedByKind, x.PlayedByIdValue })
      .HasDatabaseName("ix_pick_facts_played_by");
  }
}
