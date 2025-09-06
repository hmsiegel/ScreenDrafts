
namespace ScreenDrafts.Modules.Drafts.Infrastructure.Drafts;

internal sealed class DraftReleaseConfiguration : IEntityTypeConfiguration<DraftRelease>
{
  public void Configure(EntityTypeBuilder<DraftRelease> builder)
  {
    builder.ToTable(Tables.DraftReleases);
    
    builder.HasKey(x => new { x. PartId, x.ReleaseChannel, x.ReleaseDate });

    builder.Property(x => x.ReleaseChannel)
      .HasConversion(
      x => x.Value,
      value => ReleaseChannel.FromValue(value));

    builder.Property(x => x.ReleaseDate)
      .HasColumnType("date")
      .IsRequired();

    builder.HasIndex(x => new { x.ReleaseChannel, x.ReleaseDate });
  }
}
