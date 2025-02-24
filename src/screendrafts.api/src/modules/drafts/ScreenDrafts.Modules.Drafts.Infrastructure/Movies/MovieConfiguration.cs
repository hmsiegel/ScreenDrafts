namespace ScreenDrafts.Modules.Drafts.Infrastructure.Movies;

internal sealed class MovieConfiguration : IEntityTypeConfiguration<Movie>
{
  public void Configure(EntityTypeBuilder<Movie> builder)
  {
    builder.ToTable(Tables.Movies);

    builder.HasKey(x => x.Id);
  }
}
