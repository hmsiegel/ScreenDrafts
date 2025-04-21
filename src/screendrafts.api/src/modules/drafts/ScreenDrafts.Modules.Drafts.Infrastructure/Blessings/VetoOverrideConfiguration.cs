namespace ScreenDrafts.Modules.Drafts.Infrastructure.Blessings;

internal sealed class VetoOverrideConfiguration : IEntityTypeConfiguration<VetoOverride>
{
  public void Configure(EntityTypeBuilder<VetoOverride> builder)
  {
    builder.ToTable(Tables.VetoOverrides);

    builder.HasKey(v => v.Id);

    builder.Property(v => v.Id)
      .ValueGeneratedNever()
      .HasConversion(
      v => v.Value,
      v => new VetoOverrideId(v));

    builder.Property(v => v.VetoId)
      .IsRequired()
      .HasConversion(
      id => id.Value,
      value => VetoId.Create(value));

    builder.Property(v => v.DrafterId)
      .IsRequired(false)
      .HasConversion(
      id => id!.Value,
      value => DrafterId.Create(value));

    builder.HasOne(v => v.Veto)
      .WithOne(vo => vo.VetoOverride)
      .HasForeignKey<VetoOverride>(v => v.VetoId)
      .OnDelete(DeleteBehavior.Cascade);

    builder.HasOne(v => v.Drafter)
      .WithMany(d => d.VetoOverrides)
      .HasForeignKey(v => v.DrafterId)
      .OnDelete(DeleteBehavior.Cascade);

    builder.HasOne(v => v.DrafterTeam)
      .WithMany(dt => dt.VetoOverrides)
      .HasForeignKey(v => v.DrafterTeamId)
      .OnDelete(DeleteBehavior.Cascade);

    builder.Property(v => v.DrafterTeamId)
      .IsRequired(false)
      .HasConversion(
      id => id!.Value,
      value => DrafterTeamId.Create(value));
  }
}
