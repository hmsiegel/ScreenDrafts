namespace ScreenDrafts.Modules.Movies.Infrastructure.Genres;

internal sealed class MediaGenreConfiguration : IEntityTypeConfiguration<MediaGenre>
{
  public void Configure(EntityTypeBuilder<MediaGenre> builder)
  {
    builder.ToTable(Tables.MediaGenres);
    builder.HasKey(md => new { md.MediaId, md.GenreId });

    builder.Property(md => md.MediaId)
      .HasColumnName("media_id")
      .HasConversion(
      id => id.Value,
      value => MediaId.Create(value));

    builder.HasOne(d => d.Media)
      .WithMany(m => m.MediaGenres)
      .HasForeignKey(d => d.MediaId);

    builder.HasOne(d => d.Genre)
      .WithMany(d => d.MediaGenres)
      .HasForeignKey(d => d.GenreId);
  }
}
