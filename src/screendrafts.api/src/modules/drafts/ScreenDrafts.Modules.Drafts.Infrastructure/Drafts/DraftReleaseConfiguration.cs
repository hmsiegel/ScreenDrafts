namespace ScreenDrafts.Modules.Drafts.Infrastructure.Drafts;

internal sealed class DraftReleaseConfiguration : IEntityTypeConfiguration<DraftRelease>
{
  public void Configure(EntityTypeBuilder<DraftRelease> builder)
  {
    builder.ToTable(Tables.DraftReleases);
    
    builder.HasKey(x => new { x. PartId, x.ReleaseChannel, x.ReleaseDate });

    builder.Property(x => x.PartId)
      .HasConversion(IdConverters.DraftPartIdConverter)
      .ValueGeneratedNever();

    builder.Property(x => x.ReleaseChannel)
      .HasConversion(
      x => x.Value,
      value => ReleaseChannel.FromValue(value));

    builder.Property(x => x.EpisodeNumber);

    builder.Property(x => x.ReleaseDate)
      .HasColumnType("date")
      .HasConversion(
      d => d,
      d => d)
      .IsRequired();

    builder.HasIndex(nameof(DraftRelease.EpisodeNumber))
      .HasDatabaseName("ux_draft_releases_mainfeed_episode_number")
      .HasFilter("release_channel = 0 AND episode_number is not null")
      .IsUnique();

    builder.HasOne(x => x.DraftPart)
      .WithMany("_releases")
      .HasForeignKey(x => x.PartId)
      .OnDelete(DeleteBehavior.Cascade);

    builder.HasIndex(x => new { x.ReleaseChannel, x.ReleaseDate });
  }
}
