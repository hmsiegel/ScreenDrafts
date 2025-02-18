namespace ScreenDrafts.Modules.Movies.Infrastructure.Movies;

internal sealed class MovieGenreConfiguration : IEntityTypeConfiguration<MovieGenre>
{
  public void Configure(EntityTypeBuilder<MovieGenre> builder)
  {
    builder.ToTable(Tables.MovieGenres);

    builder.HasKey(md => new { md.MovieId, md.GenreId });

    builder.Property(md => md.MovieId)
      .HasColumnName("movie_id")
      .HasConversion(
      id => id.Value,
      value => MovieId.Create(value));

    builder.HasOne(d => d.Movie)
      .WithMany(m => m.MovieGenres)
      .HasForeignKey(d => d.MovieId);

    builder.HasOne(d => d.Genre)
      .WithMany(d => d.MovieGenres)
      .HasForeignKey(d => d.GenreId);
  }
}
