namespace ScreenDrafts.Modules.Movies.Infrastructure.Movies;

internal sealed class MovieConfiguration : IEntityTypeConfiguration<Movie>
{
  public void Configure(EntityTypeBuilder<Movie> builder)
  {
    builder.ToTable(Tables.Movies);

    builder.HasKey(t => t.Id);

    builder.Property(d => d.Id)
      .ValueGeneratedNever()
      .HasConversion(
      id => id.Value,
      value => MovieId.Create(value));

    builder.HasIndex(d => d.ImdbId);

    builder.Property(m => m.TmdbId);

    builder.HasIndex(m => m.TmdbId);

    builder.Property(d => d.Plot)
      .HasColumnType("text");
  }
}
