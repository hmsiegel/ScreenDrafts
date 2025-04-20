namespace ScreenDrafts.Modules.Drafts.Infrastructure.Vetoes;
internal sealed class VetoConfiguration : IEntityTypeConfiguration<Veto>
{
  public void Configure(EntityTypeBuilder<Veto> builder)
  {
    builder.ToTable(Tables.Vetoes);

    builder.HasKey(veto => veto.Id);

    builder.Property(veto => veto.Id)
      .ValueGeneratedNever()
      .HasConversion(
      v => v.Value,
      v => VetoId.Create(v));

    builder.HasOne(v => v.VetoOverride)
      .WithOne(vo => vo.Veto)
      .HasForeignKey<VetoOverride>(v => v.VetoId);

    builder.HasOne(v => v.Pick)
      .WithOne(p => p.Veto)
      .HasForeignKey<Veto>(v => v.PickId)
      .OnDelete(DeleteBehavior.Cascade);

    builder.Property(v => v.DrafterTeamId)
      .IsRequired(false)
      .HasConversion(
      id => id!.Value,
      value => DrafterTeamId.Create(value));

    builder.Property(v => v.DrafterId)
      .IsRequired(false)
      .HasConversion(
      id => id!.Value,
      value => DrafterId.Create(value));

    builder.HasOne(v => v.Drafter)
      .WithMany(d => d.Vetoes)
      .HasForeignKey(v => v.DrafterId)
      .OnDelete(DeleteBehavior.Cascade);

    builder.HasOne(v => v.DrafterTeam)
      .WithMany(dt => dt.Vetoes)
      .HasForeignKey(v => v.DrafterTeamId)
      .OnDelete(DeleteBehavior.Cascade);
  }
}
