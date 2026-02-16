namespace ScreenDrafts.Modules.Drafts.Infrastructure.People;

internal sealed class HostConfiguration : IEntityTypeConfiguration<Host>
{
  public void Configure(EntityTypeBuilder<Host> builder)
  {
    builder.ToTable(Tables.Hosts);

    builder.HasKey(x => x.Id);

    builder.Property(x => x.Id)
      .ValueGeneratedNever()
      .HasConversion(IdConverters.HostIdConverter);

    builder.Property(d => d.PublicId)
      .IsRequired()
      .HasMaxLength(PublicIdPrefixes.MaxPublicIdLength);

    builder.HasIndex(d => d.PublicId)
      .IsUnique()
      .HasDatabaseName("ix_hosts_public_id");

    builder.Property(h => h.PersonId)
      .IsRequired()
      .HasColumnName("person_id")
      .HasConversion(IdConverters.PersonIdConverter);

    builder.HasIndex(h => h.PersonId)
      .IsUnique();

    builder.HasOne(h => h.Person)
      .WithOne(p => p.HostProfile)
      .HasForeignKey<Host>(h => h.PersonId)
      .OnDelete(DeleteBehavior.Cascade);

    builder.Navigation(h => h.Drafts)
      .HasField("_drafts")
      .UsePropertyAccessMode(PropertyAccessMode.Field);
  }
}
