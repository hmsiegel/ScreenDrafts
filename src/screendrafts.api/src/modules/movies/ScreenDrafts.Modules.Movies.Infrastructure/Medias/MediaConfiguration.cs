namespace ScreenDrafts.Modules.Movies.Infrastructure.Medias;

internal sealed class MediaConfiguration : IEntityTypeConfiguration<Media>
{
  public void Configure(EntityTypeBuilder<Media> builder)
  {
    builder.ToTable(Tables.Media);

    builder.HasKey(t => t.Id);

    builder.Property(d => d.Id)
      .ValueGeneratedNever()
      .HasConversion(
      id => id.Value,
      value => MediaId.Create(value));

    builder.Property(x => x.PublicId)
      .IsRequired()
      .HasMaxLength(PublicIdPrefixes.MaxPublicIdLength)
      .ValueGeneratedNever();

    builder.HasIndex(x => x.PublicId)
      .IsUnique()
      .HasDatabaseName("ux_media_public_id");

    builder.HasIndex(d => d.ImdbId);

    builder.Property(m => m.TmdbId);

    builder.HasIndex(m => m.TmdbId);

    builder.Property(m => m.IgdbId);

    builder.HasIndex(m => m.IgdbId);

    builder.Property(d => d.Plot)
      .HasColumnType("text");
  }
}
