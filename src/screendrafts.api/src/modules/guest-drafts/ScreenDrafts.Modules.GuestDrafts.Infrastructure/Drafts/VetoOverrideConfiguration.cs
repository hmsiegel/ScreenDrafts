namespace ScreenDrafts.Modules.GuestDrafts.Infrastructure.Drafts;

internal sealed class VetoOverrideConfiguration : IEntityTypeConfiguration<VetoOverride>
{
  public void Configure(EntityTypeBuilder<VetoOverride> builder)
  {
    builder.ToTable(Tables.VetoOverrides);

    builder.HasKey(vo => vo.Id);

    builder
      .Property(vo => vo.Id)
      .ValueGeneratedNever()
      .HasColumnName("id")
      .HasConversion(IdConverters.VetoOverrideIdConverter);

    builder.Property(vo => vo.VetoId).IsRequired().HasConversion(IdConverters.VetoIdConverter);

    // Veto <-> VetoOverride 1:1, containment, cascades. Configured from this side.
    builder
      .HasOne(vo => vo.Veto)
      .WithOne(v => v.VetoOverride)
      .HasForeignKey<VetoOverride>(vo => vo.VetoId)
      .OnDelete(DeleteBehavior.Cascade);

    // IssuedByParticipant -- reference, not containment, Restrict.
    builder
      .Property(vo => vo.IssuedByParticipantId)
      .IsRequired()
      .HasConversion(IdConverters.DraftParticipantIdConverter);

    builder
      .HasOne(vo => vo.IssuedByParticipant)
      .WithMany()
      .HasForeignKey(vo => vo.IssuedByParticipantId)
      .OnDelete(DeleteBehavior.Restrict);

    builder.Property(vo => vo.ActedByPublicId).HasMaxLength(PublicIdPrefixes.MaxPublicIdLength);

    builder.Property(vo => vo.SpentFromFungiblePool).IsRequired();

    builder.Property(vo => vo.Note).HasMaxLength(500);
  }
}
