using ScreenDrafts.Modules.GuestDrafts.Domain.Drafts.Entities;

namespace ScreenDrafts.Modules.GuestDrafts.Infrastructure.Drafts;

internal sealed class VetoConfiguration : IEntityTypeConfiguration<Veto>
{
  public void Configure(EntityTypeBuilder<Veto> builder)
  {
    builder.ToTable(Tables.Vetoes);

    builder.HasKey(v => v.Id);

    builder
      .Property(v => v.Id)
      .ValueGeneratedNever()
      .HasColumnName("id")
      .HasConversion(IdConverters.VetoIdConverter);

    builder.Property(v => v.TargetPickId).IsRequired().HasConversion(IdConverters.PickIdConverter);

    builder.Property(v => v.Sequence).IsRequired();

    builder.HasIndex(v => new { v.TargetPickId, v.Sequence }).IsUnique();

    // IssuedByParticipant -- reference, not containment, Restrict.
    builder
      .Property(v => v.IssuedByParticipantId)
      .IsRequired()
      .HasConversion(IdConverters.DraftParticipantIdConverter);

    builder
      .HasOne(v => v.IssuedByParticipant)
      .WithMany()
      .HasForeignKey(v => v.IssuedByParticipantId)
      .OnDelete(DeleteBehavior.Restrict);

    builder.Property(v => v.ActedByPublicId).HasMaxLength(PublicIdPrefixes.MaxPublicIdLength);

    builder.Property(v => v.SpentFromFungiblePool).IsRequired();

    builder.Property(v => v.IsOverridden).IsRequired();

    // VetoOverride -- optional 1:1, containment, cascades. Configured from the
    // GuestDraftVetoOverride side (HasOne there points back at this Veto) to avoid
    // configuring the same relationship from both ends.

    builder.Property(v => v.OccurredOn).IsRequired();

    builder.Property(v => v.Note).HasMaxLength(500);
  }
}
