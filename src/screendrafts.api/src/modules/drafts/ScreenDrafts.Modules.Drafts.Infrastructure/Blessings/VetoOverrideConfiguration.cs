namespace ScreenDrafts.Modules.Drafts.Infrastructure.Blessings;

internal sealed class VetoOverrideConfiguration : IEntityTypeConfiguration<VetoOverride>
{
  public void Configure(EntityTypeBuilder<VetoOverride> builder)
  {
    builder.ToTable(Tables.VetoOverrides);

    // Id
    builder.HasKey(v => v.Id);

    builder.Property(v => v.Id)
      .ValueGeneratedNever()
      .HasConversion(IdConverters.VetoOverrideIdConverter);

    // Target Veto
    builder.Property(x => x.VetoId)
      .IsRequired()
      .ValueGeneratedNever()
      .HasConversion(IdConverters.VetoIdConverter);

    // Issued By Participant
    builder.Property(x => x.IssuedByParticipantId)
      .IsRequired()
      .ValueGeneratedNever()
      .HasConversion(IdConverters.DraftPartParticipantIdConverter);

    builder.HasOne(x => x.IssuedByParticipant)
      .WithMany()
      .HasForeignKey(x => x.IssuedByParticipantId)
      .OnDelete(DeleteBehavior.Restrict);

    builder.HasIndex(x => x.VetoId).IsUnique();
  }
}
