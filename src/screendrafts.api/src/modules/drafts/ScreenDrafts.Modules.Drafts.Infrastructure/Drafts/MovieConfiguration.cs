
namespace ScreenDrafts.Modules.Drafts.Infrastructure.Drafts;

internal sealed class MovieConfiguration : IEntityTypeConfiguration<Movie>
{
  public void Configure(EntityTypeBuilder<Movie> builder)
  {
    builder.ToTable(Tables.Movies);

    builder.HasKey(x => x.Id);

    builder.Property(x => x.Id)
        .HasConversion(
            x => x.Value,
            x => new MovieId(x));

    builder.Property(x => x.MovieTitle)
        .HasConversion(
            x => x.Value,
            x => new MovieTitle(x));
  }
}
