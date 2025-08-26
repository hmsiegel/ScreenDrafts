namespace ScreenDrafts.Modules.Drafts.Infrastructure.People;

internal sealed class HostConfiguration : IEntityTypeConfiguration<Host>
{
  public void Configure(EntityTypeBuilder<Host> builder)
  {
    builder.ToTable(Tables.Hosts);

    builder.HasKey(x => x.Id);

    builder.Property(h => h.ReadableId)
      .ValueGeneratedOnAdd();

    builder.Property(x => x.Id)
      .ValueGeneratedNever()
      .HasConversion(
      h => h.Value,
      value => HostId.Create(value));

    builder.HasOne(h => h.Person)
      .WithOne(p => p.HostProfile)
      .HasForeignKey<Host>(h => h.PersonId);
  }
}
