namespace ScreenDrafts.Modules.Drafts.Infrastructure.Vetoes;
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

    builder.Property(v => v.IsUsed)
      .IsRequired();
  }
}
