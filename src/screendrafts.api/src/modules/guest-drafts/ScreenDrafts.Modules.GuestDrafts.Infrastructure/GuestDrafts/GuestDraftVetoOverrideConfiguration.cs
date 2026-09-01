namespace ScreenDrafts.Modules.GuestDrafts.Infrastructure.GuestDrafts;

internal sealed class GuestDraftVetoOverrideConfiguration
  : IEntityTypeConfiguration<GuestDraftVetoOverride>
{
  public void Configure(EntityTypeBuilder<GuestDraftVetoOverride> builder)
  {
    builder.ToTable(Tables.GuestDraftVetoOverrides);

    builder.HasKey(vo => vo.Id);

    builder.Property(vo => vo.Id)
      .ValueGeneratedNever()
      .HasColumnName("id")
      .HasConversion(IdConverters.GuestDraftVetoOverrideIdConverter);

    builder.Property(vo => vo.VetoId)
      .IsRequired()
      .HasConversion(IdConverters.GuestDraftVetoIdConverter);

    // Veto <-> VetoOverride 1:1, containment, cascades. Configured from this side.
    builder.HasOne(vo => vo.Veto)
      .WithOne(v => v.VetoOverride)
      .HasForeignKey<GuestDraftVetoOverride>(vo => vo.VetoId)
      .OnDelete(DeleteBehavior.Cascade);

    // IssuedByParticipant -- reference, not containment, Restrict.
    builder.Property(vo => vo.IssuedByParticipantId)
      .IsRequired()
      .HasConversion(IdConverters.GuestDraftParticipantIdConverter);

    builder.HasOne(vo => vo.IssuedByParticipant)
      .WithMany()
      .HasForeignKey(vo => vo.IssuedByParticipantId)
      .OnDelete(DeleteBehavior.Restrict);

    builder.Property(vo => vo.ActedByPublicId)
      .HasMaxLength(PublicIdPrefixes.MaxPublicIdLength);

    builder.Property(vo => vo.SpentFromFungiblePool)
      .IsRequired();

    builder.Property(vo => vo.Note)
      .HasMaxLength(500);
  }
}
