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

    builder.OwnsMany(x => x.Versions, mvb =>
    {
      mvb.ToTable(Tables.MovieVersions);

      mvb.WithOwner().HasForeignKey("movie_id");

      mvb.Property<Guid>("id")
      .ValueGeneratedNever();

      mvb.HasKey("id");

      mvb.Property(v => v.Name)
      .IsRequired()
      .HasMaxLength(100)
      .HasColumnName("name");

      mvb.HasIndex("movie_id", nameof(MovieVersion.Name))
      .IsUnique();
    });

    builder.Navigation(x => x.Versions)
      .HasField("_versions")
      .UsePropertyAccessMode(PropertyAccessMode.Field);
  }
}
