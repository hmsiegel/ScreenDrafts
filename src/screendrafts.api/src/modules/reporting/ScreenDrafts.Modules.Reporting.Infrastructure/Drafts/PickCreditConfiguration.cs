namespace ScreenDrafts.Modules.Reporting.Infrastructure.Drafts;

internal sealed class PickCreditFactConfiguration : IEntityTypeConfiguration<PickCreditFact>
{
  public void Configure(EntityTypeBuilder<PickCreditFact> builder)
  {
    builder.ToTable(Tables.PickCreditFacts);

    builder.HasKey(x => x.Id);
    builder.Property(x => x.Id).ValueGeneratedNever();

    builder.Property(x => x.PickId);
    builder.Property(x => x.DraftId);
    builder.Property(x => x.DraftPartPublicId);
    builder.Property(x => x.DrafterIdValue);
    builder.Property(x => x.DrafterPublicId);
    builder.Property(x => x.DrafterName);
    builder.Property(x => x.RecordedAtUtc);

    builder
      .HasIndex(x => new { x.PickId, x.DrafterIdValue })
      .IsUnique()
      .HasDatabaseName("ux_pick_credit_facts_pick_id_drafter_id_value");
    builder
      .HasIndex(x => x.DrafterIdValue)
      .HasDatabaseName("ix_pick_credit_facts_drafter_id_value");
    builder
      .HasIndex(x => x.DraftPartPublicId)
      .HasDatabaseName("ix_pick_credit_facts_draft_part_public_id");
    builder.HasIndex(x => x.DraftId).HasDatabaseName("ix_pick_credit_facts_draft_id");
  }
}
