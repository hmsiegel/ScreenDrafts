namespace ScreenDrafts.Modules.Drafts.Infrastructure.DraftParts;

internal sealed class DraftReleaseConfiguration : IEntityTypeConfiguration<DraftRelease>
{
  public void Configure(EntityTypeBuilder<DraftRelease> builder)
  {
    builder.ToTable(Tables.DraftReleases);

    builder.HasKey(x => new { x. PartId, x.ReleaseChannel});

    builder.Property(x => x.PartId)
      .HasConversion(IdConverters.DraftPartIdConverter)
      .ValueGeneratedNever();

    builder.Property(x => x.ReleaseChannel)
      .HasConversion(
      x => x.Value,
      value => ReleaseChannel.FromValue(value));

    builder.Property(x => x.ReleaseDate)
      .HasColumnType("date")
      .HasConversion(
      d => d,
      d => d)
      .IsRequired();

    builder.HasOne(x => x.DraftPart)
      .WithMany("_releases")
      .HasForeignKey(x => x.PartId)
      .OnDelete(DeleteBehavior.Cascade);

    builder.HasIndex(x => new { x.ReleaseChannel, x.ReleaseDate });
  }
}
