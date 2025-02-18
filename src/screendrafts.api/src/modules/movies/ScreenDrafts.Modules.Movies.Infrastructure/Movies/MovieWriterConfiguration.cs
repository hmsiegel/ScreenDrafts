namespace ScreenDrafts.Modules.Movies.Infrastructure.Movies;

internal sealed class MovieWriterConfiguration : IEntityTypeConfiguration<MovieWriter>
{
  public void Configure(EntityTypeBuilder<MovieWriter> builder)
  {
    builder.ToTable(Tables.MovieWriters);

    builder.HasKey(md => new { md.MovieId, md.WriterId });

    builder.Property(md => md.MovieId)
      .HasColumnName("movie_id")
      .HasConversion(
      id => id.Value,
      value => MovieId.Create(value));

    builder.Property(md => md.WriterId)
      .HasColumnName("writer_id")
      .HasConversion(
      id => id.Value,
      value => PersonId.Create(value));

    builder.HasOne(d => d.Movie)
      .WithMany(m => m.MovieWriters)
      .HasForeignKey(d => d.MovieId);

    builder.HasOne(d => d.Writer)
      .WithMany(d => d.MovieWriters)
      .HasForeignKey(d => d.WriterId);
  }
}
