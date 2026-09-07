namespace ScreenDrafts.Modules.GuestDrafts.Infrastructure.GuestDrafts;

internal sealed class GuestDraftVetoConfiguration : IEntityTypeConfiguration<GuestDraftVeto>
{
  public void Configure(EntityTypeBuilder<GuestDraftVeto> builder)
  {
    builder.ToTable(Tables.GuestDraftVetoes);

    builder.HasKey(v => v.Id);

    builder.Property(v => v.Id)
      .ValueGeneratedNever()
      .HasColumnName("id")
      .HasConversion(IdConverters.GuestDraftVetoIdConverter);

    builder.Property(v => v.TargetPickId)
      .IsRequired()
      .HasConversion(IdConverters.GuestDraftPickIdConverter);

    builder.Property(v => v.Sequence)
      .IsRequired();

    builder.HasIndex(v => new { v.TargetPickId, v.Sequence })
      .IsUnique();

    // IssuedByParticipant -- reference, not containment, Restrict.
    builder.Property(v => v.IssuedByParticipantId)
      .IsRequired()
      .HasConversion(IdConverters.GuestDraftParticipantIdConverter);

    builder.HasOne(v => v.IssuedByParticipant)
      .WithMany()
      .HasForeignKey(v => v.IssuedByParticipantId)
      .OnDelete(DeleteBehavior.Restrict);

    builder.Property(v => v.ActedByPublicId)
      .HasMaxLength(PublicIdPrefixes.MaxPublicIdLength);

    builder.Property(v => v.SpentFromFungiblePool)
      .IsRequired();

    builder.Property(v => v.IsOverridden)
      .IsRequired();

    // VetoOverride -- optional 1:1, containment, cascades. Configured from the
    // GuestDraftVetoOverride side (HasOne there points back at this Veto) to avoid
    // configuring the same relationship from both ends.

    builder.Property(v => v.OccurredOn)
      .IsRequired();

    builder.Property(v => v.Note)
      .HasMaxLength(500);
  }
}
