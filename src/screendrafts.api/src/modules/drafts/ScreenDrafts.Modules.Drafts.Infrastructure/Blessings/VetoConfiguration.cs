namespace ScreenDrafts.Modules.Drafts.Infrastructure.Blessings;

internal sealed class VetoConfiguration : IEntityTypeConfiguration<Veto>
{
  public void Configure(EntityTypeBuilder<Veto> builder)
  {
    builder.ToTable(Tables.Vetoes);

    // Id
    builder.HasKey(veto => veto.Id);

    builder.Property(veto => veto.Id)
      .ValueGeneratedNever()
      .HasConversion(IdConverters.VetoIdConverter);

    // IssuedBy
    builder.Property(x => x.IssuedByParticipantId)
      .IsRequired()
      .ValueGeneratedNever()
      .HasConversion(IdConverters.DraftPartParticipantIdConverter);

    builder.HasOne(x => x.IssuedByParticipant)
      .WithMany(p => p.Vetoes)
      .HasForeignKey(x => x.IssuedByParticipantId)
      .OnDelete(DeleteBehavior.Restrict);

    // Target Pick
    builder.Property(x => x.TargetPickId)
      .IsRequired()
      .ValueGeneratedNever()
      .HasConversion(IdConverters.DraftPickIdConverter);

    builder.HasOne(x => x.TargetPick)
      .WithOne(p => p.Veto)
      .HasForeignKey<Veto>(x => x.TargetPickId)
      .OnDelete(DeleteBehavior.Cascade);


    builder.Property(v => v.IsOverridden)
      .IsRequired();

    builder.Property(v => v.OccurredOn)
      .IsRequired();

    builder.Property(v => v.Note)
      .HasMaxLength(1000);

    builder.HasOne(v => v.VetoOverride)
      .WithOne(vo => vo.Veto)
      .HasForeignKey<VetoOverride>(v => v.VetoId)
      .OnDelete(DeleteBehavior.Cascade);

    builder.Ignore(v => v.DraftPart);
    builder.Ignore(v => v.DraftPartId);

    builder.HasIndex(x => new {x.IssuedByParticipantId, x.TargetPickId }).IsUnique();
  }
}
