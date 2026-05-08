namespace ScreenDrafts.Modules.Reporting.Infrastructure.Drafts;

internal sealed class DraftPartReleaseConfiguration : IEntityTypeConfiguration<DraftPartRelease>
{
  public void Configure(EntityTypeBuilder<DraftPartRelease> builder)
  {
    builder.ToTable(Tables.DraftPartReleases);

    builder.HasKey(x => x.Id);

    builder
      .Property(x => x.Id)
      .ValueGeneratedNever()
      .HasConversion(IdConverters.DraftPartReleaseIdConverter);

    builder.Property(x => x.DraftId).IsRequired();

    builder.HasIndex(x => x.DraftId).HasDatabaseName("ix_draft_part_releases_draft_id");

    builder.Property(x => x.DraftPartPublicId).IsRequired();

    builder.Property(x => x.ReleaseChannel).IsRequired();

    builder.Property(x => x.ReleaseDate);

    builder
      .HasIndex(x => new { x.DraftPartPublicId, x.ReleaseChannel })
      .IsUnique()
      .HasDatabaseName("ux_draft_part_releases_part_public_id_channel");
  }
}
