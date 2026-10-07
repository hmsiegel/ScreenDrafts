namespace ScreenDrafts.Modules.Reporting.Infrastructure.Drafts;

internal sealed class VetoFactConfiguration : IEntityTypeConfiguration<VetoFact>
{
  public void Configure(EntityTypeBuilder<VetoFact> builder)
  {
    builder.ToTable(Tables.VetoFacts);

    builder.HasKey(x => x.Id);
    builder.Property(x => x.Id).ValueGeneratedNever();

    builder.Property(x => x.PickId);
    builder.Property(x => x.DraftId);
    builder.Property(x => x.DraftPartPublicId);
    builder.Property(x => x.Sequence);
    builder.Property(x => x.IssuedByKind);
    builder.Property(x => x.IssuedByIdValue);
    builder.Property(x => x.IssuedByPublicId);
    builder.Property(x => x.IssuedByName);
    builder.Property(x => x.IsOverridden);
    builder.Property(x => x.OverriddenByKind);
    builder.Property(x => x.OverriddenByIdValue);
    builder.Property(x => x.OverriddenByPublicId);
    builder.Property(x => x.OverriddenByName);
    builder.Property(x => x.IsSelfVeto);
    builder.Property(x => x.RecordedAtUtc);

    builder.HasIndex(x => x.PickId).HasDatabaseName("ix_veto_facts_pick_id");
    builder
      .HasIndex(x => x.DraftPartPublicId)
      .HasDatabaseName("ix_veto_facts_draft_part_public_id");
    builder.HasIndex(x => x.DraftId).HasDatabaseName("ix_veto_facts_draft_id");
    builder
      .HasIndex(x => new { x.IssuedByKind, x.IssuedByIdValue })
      .HasDatabaseName("ix_veto_facts_issued_by");
  }
}
