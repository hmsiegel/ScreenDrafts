namespace ScreenDrafts.Modules.Drafts.Infrastructure.Drafters;

internal sealed class HostConfiguration : IEntityTypeConfiguration<Host>
{
  public void Configure(EntityTypeBuilder<Host> builder)
  {
    builder.ToTable(Tables.Hosts);

    builder.HasKey(x => x.Id);

    builder.Property(x => x.Id)
      .ValueGeneratedNever();

    builder.HasMany(h => h.HostedDrafts)
      .WithMany(d => d.Hosts);
  }
}
