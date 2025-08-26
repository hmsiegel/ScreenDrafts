namespace ScreenDrafts.Modules.Drafts.Infrastructure.Drafts;

internal sealed class DraftHostConfiguration : IEntityTypeConfiguration<DraftHost>
{
  public void Configure(EntityTypeBuilder<DraftHost> builder)
  {
    builder.ToTable(Tables.DraftsHosts);

    builder.HasKey(dh => new { dh.DraftId, dh.HostId });

    builder.Property(dh => dh.DraftId)
      .IsRequired()
      .HasConversion(
        id => id.Value,
        value => DraftId.Create(value));

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

    builder.HasOne(dh => dh.Draft)
      .WithMany(d => d.DraftHosts)
      .HasForeignKey(x => x.DraftId)
      .HasPrincipalKey(d => d.Id)
      .OnDelete(DeleteBehavior.Cascade);

    builder.HasOne(dh => dh.Host)
      .WithMany(h => h.Drafts)
      .HasForeignKey(x => x.HostId)
      .HasPrincipalKey(h => h.Id)
      .OnDelete(DeleteBehavior.Restrict);

    builder.HasIndex(x => x.DraftId)
      .HasDatabaseName("ux_draft_hosts_one_primary_per_draft")
      .HasFilter("role = 0")
      .IsUnique();
  }
}
