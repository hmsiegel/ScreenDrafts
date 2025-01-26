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

    builder.Property(v => v.IsUsed)
      .IsRequired();

    builder.HasOne(v => v.VetoOverride)
      .WithOne(vo => vo.Veto)
      .HasForeignKey<VetoOverride>(v => v.VetoId);
  }
}
