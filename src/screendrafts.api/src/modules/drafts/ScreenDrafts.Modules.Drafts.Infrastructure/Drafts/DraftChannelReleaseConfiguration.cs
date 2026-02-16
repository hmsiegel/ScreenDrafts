namespace ScreenDrafts.Modules.Drafts.Infrastructure.Drafts;

internal sealed class DraftChannelReleaseConfiguration : IEntityTypeConfiguration<DraftChannelRelease>
{
  public void Configure(EntityTypeBuilder<DraftChannelRelease> builder)
  {
    builder.ToTable(Tables.DraftChannelReleases);

    builder.HasKey(x => new { x.DraftId, x.ReleaseChannel });

    builder.Property(x => x.DraftId)
      .IsRequired()
      .ValueGeneratedNever()
      .HasConversion(IdConverters.DraftIdConverter);

    builder.Property(x => x.ReleaseChannel)
      .IsRequired()
      .HasConversion(
        x => x.Value,
        x => ReleaseChannel.FromValue(x));

    builder.Property(x => x.SeriesId)
      .IsRequired()
      .ValueGeneratedNever()
      .HasConversion(IdConverters.SeriesIdConverter);

    builder.Property(x => x.EpisodeNumber);

    builder.Property(x => x.CreatedOnUtc);

    builder.HasOne(x => x.Draft)
      .WithMany(d => d.ChannelReleases)
      .HasForeignKey(x => x.DraftId)
      .OnDelete(DeleteBehavior.Cascade);

    builder.HasOne(x => x.Series)
      .WithMany()
      .HasForeignKey(x => x.SeriesId)
      .OnDelete(DeleteBehavior.Cascade);

    builder.HasIndex(x => new { x.ReleaseChannel, x.EpisodeNumber, x.SeriesId })
      .HasDatabaseName("ux_draft_channel_releases_channel_series_episode_number")
      .HasFilter($"episode_number is not null")
      .IsUnique();
  }
}
