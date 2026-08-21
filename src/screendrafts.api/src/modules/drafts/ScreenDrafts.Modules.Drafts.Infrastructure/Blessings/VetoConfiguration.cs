namespace ScreenDrafts.Modules.Drafts.Infrastructure.Blessings;

internal sealed class VetoConfiguration : IEntityTypeConfiguration<Veto>
{
  public void Configure(EntityTypeBuilder<Veto> builder)
  {
    builder.ToTable(Tables.Vetoes);

    // Id
    builder.HasKey(veto => veto.Id);

    builder
      .Property(veto => veto.Id)
      .ValueGeneratedNever()
      .HasConversion(IdConverters.VetoIdConverter);

    // IssuedBy
    builder
      .Property(x => x.IssuedByParticipantId)
      .IsRequired()
      .ValueGeneratedNever()
      .HasConversion(IdConverters.DraftPartParticipantIdConverter);

    // Target Pick
    // A pick may accumulate more than one veto over its lifetime (veto -> override -> re-veto),
    // so this is configured from this side as the "many" end. Pick no longer
    // configures the inverse of this relationship. See <see cref="PickConfiguration"/> for more details.
    builder
      .Property(x => x.TargetPickId)
      .IsRequired()
      .ValueGeneratedNever()
      .HasConversion(IdConverters.DraftPickIdConverter);

    builder
      .HasOne(x => x.TargetPick)
      .WithMany("_vetoes")
      .HasForeignKey(x => x.TargetPickId)
      .OnDelete(DeleteBehavior.Cascade);

    builder.Property(v => v.Sequence).IsRequired();

    builder.Property(v => v.IsOverridden).IsRequired();

    builder.Property(v => v.SpentFromFungiblePool).IsRequired();

    builder.Property(v => v.OccurredOn).IsRequired();

    builder.Property(v => v.Note).HasMaxLength(1000);

    builder
      .HasOne(v => v.VetoOverride)
      .WithOne(vo => vo.Veto)
      .HasForeignKey<VetoOverride>(v => v.VetoId)
      .OnDelete(DeleteBehavior.Cascade);

    builder.Ignore(v => v.DraftPart);
    builder.Ignore(v => v.DraftPartId);

    builder.HasIndex(x => new { x.TargetPickId, x.Sequence }).IsUnique();

    builder
      .Property(v => v.SubDraftId)
      .IsRequired(required: false)
      .HasConversion(IdConverters.NullableSubDraftIdConverter);
  }
}
