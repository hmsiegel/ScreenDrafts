namespace ScreenDrafts.Modules.Drafts.Infrastructure.Movies;

internal sealed class MovieConfiguration : IEntityTypeConfiguration<Movie>
{
  public void Configure(EntityTypeBuilder<Movie> builder)
  {
    builder.ToTable(Tables.Movies);

    builder.HasKey(x => x.Id);

    builder.Property(x => x.MovieTitle)
      .IsRequired();

    builder.Property(x => x.ImdbId)
      .IsRequired();
  }
}
