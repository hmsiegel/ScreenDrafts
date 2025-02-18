namespace ScreenDrafts.Modules.Movies.Infrastructure.Movies;

internal sealed class MovieDirectorConfiguration : IEntityTypeConfiguration<MovieDirector>
{
  public void Configure(EntityTypeBuilder<MovieDirector> builder)
  {
    builder.ToTable(Tables.MovieDirectors);

    builder.HasKey(md => new { md.MovieId, md.DirectorId });

    builder.Property(md => md.MovieId)
      .HasColumnName("movie_id")
      .HasConversion(
      id => id.Value,
      value => MovieId.Create(value));

    builder.Property(md => md.DirectorId)
      .HasColumnName("director_id")
      .HasConversion(
      id => id.Value,
      value => PersonId.Create(value));

    builder.HasOne(d => d.Movie)
      .WithMany(m => m.MovieDirectors)
      .HasForeignKey(d => d.MovieId);

    builder.HasOne(d => d.Director)
      .WithMany(d => d.MovieDirectors)
      .HasForeignKey(d => d.DirectorId);
  }
}
