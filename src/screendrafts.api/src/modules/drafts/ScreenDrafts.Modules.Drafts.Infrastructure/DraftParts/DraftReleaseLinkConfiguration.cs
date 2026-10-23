namespace ScreenDrafts.Modules.Drafts.Infrastructure.DraftParts;

internal sealed class DraftReleaseLinkConfiguration : IEntityTypeConfiguration<DraftReleaseLink>
{
  public void Configure(EntityTypeBuilder<DraftReleaseLink> builder)
  {
    builder.ToTable(Tables.DraftReleaseLinks);

    builder.HasKey(x => new
    {
      x.PartId,
      x.ReleaseChannel,
      x.Platform,
    });

    builder
      .Property(x => x.PartId)
      .HasConversion(IdConverters.DraftPartIdConverter)
      .ValueGeneratedNever();

    builder
      .Property(x => x.ReleaseChannel)
      .HasConversion(x => x.Value, value => ReleaseChannel.FromValue(value));

    builder
      .Property(x => x.Platform)
      .HasConversion(x => x.Value, value => ReleaseLinkPlatform.FromValue(value));

    builder
      .Property(x => x.Status)
      .HasConversion(x => x.Value, value => ReleaseLinkStatus.FromValue(value))
      .IsRequired();

    builder.Property(x => x.Url).HasMaxLength(500);

    builder.Property(x => x.NextCheckAtUtc);

    builder.Property(x => x.Attempts).IsRequired();

    builder.Property(x => x.UpdatedOnUtc).IsRequired();

    builder
      .HasOne(x => x.Release)
      .WithMany("_links")
      .HasForeignKey(x => new { x.PartId, x.ReleaseChannel })
      .OnDelete(DeleteBehavior.Cascade);

    builder.HasIndex(x => new { x.Status, x.NextCheckAtUtc });
  }
}
