namespace ScreenDrafts.Modules.Drafts.Infrastructure.DraftParts;

internal sealed class DraftHostConfiguration : IEntityTypeConfiguration<DraftHost>
{
  public void Configure(EntityTypeBuilder<DraftHost> builder)
  {
    builder.ToTable(Tables.DraftsHosts);

    builder.HasKey(dh => new { dh.DraftPartId, dh.HostId });

    builder.Property(dh => dh.DraftPartId)
      .IsRequired()
      .HasConversion(IdConverters.DraftPartIdConverter);

    builder.Property(dh => dh.HostId)
      .IsRequired()
      .HasConversion(IdConverters.HostIdConverter);

    builder.Property(dh => dh.Role)
      .HasColumnName("role")
      .HasConversion(
        role => role.Value,
        value => HostRole.FromValue(value))
      .IsRequired();

    builder.HasOne(dh => dh.Host)
      .WithMany(h => h.Drafts)
      .HasForeignKey(x => x.HostId)
      .OnDelete(DeleteBehavior.Restrict);

    builder.Navigation(dh => dh.Host)
      .AutoInclude(false);

    var primaryRoleValue = HostRole.Primary.Value;

    builder.HasIndex(x => x.DraftPartId)
      .HasDatabaseName("ux_draft_hosts_one_primary_per_draft_part")
      .HasFilter($"role = {primaryRoleValue}")
      .IsUnique();
  }
}
