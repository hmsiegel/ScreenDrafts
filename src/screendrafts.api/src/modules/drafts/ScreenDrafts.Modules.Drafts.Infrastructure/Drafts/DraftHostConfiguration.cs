namespace ScreenDrafts.Modules.Drafts.Infrastructure.Drafts;

internal sealed class DraftHostConfiguration : IEntityTypeConfiguration<DraftHost>
{
  public void Configure(EntityTypeBuilder<DraftHost> builder)
  {
    builder.ToTable(Tables.DraftsHosts);

    builder.HasKey(dh => new { dh.DraftPartId, dh.HostId });

    builder.Property(dh => dh.DraftPartId)
      .IsRequired()
      .HasConversion(
        id => id.Value,
        value => DraftPartId.Create(value));

    builder.Property(dh => dh.HostId)
      .IsRequired()
      .HasConversion(
        id => id.Value,
        value => HostId.Create(value));

    builder.Property(dh => dh.Role)
      .HasConversion(
        role => role.Value,
        value => HostRole.FromValue(value))
      .IsRequired();

    builder.HasOne(dh => dh.DraftPart)
      .WithMany(d => d.DraftHosts)
      .HasForeignKey(x => x.DraftPartId)
      .HasPrincipalKey(d => d.Id)
      .OnDelete(DeleteBehavior.Cascade);

    builder.HasOne(dh => dh.Host)
      .WithMany(h => h.Drafts)
      .HasForeignKey(x => x.HostId)
      .HasPrincipalKey(h => h.Id)
      .OnDelete(DeleteBehavior.Restrict);

    builder.HasIndex(x => x.DraftPartId)
      .HasDatabaseName("ux_draft_hosts_one_primary_per_draft")
      .HasFilter("role = 0")
      .IsUnique();
  }
}
