namespace ScreenDrafts.Infrastructure.Persistence.Configuration;
public sealed class MovieConfiguration : IEntityTypeConfiguration<Movie>
{
    public void Configure(EntityTypeBuilder<Movie> builder)
    {
        ConfigureMoviesTable(builder);
        ConfigureMoviesDraftsSelectedTable(builder);
        ConfigureMoviesDraftsVetoedTable(builder);
    }

    private static void ConfigureMoviesDraftsVetoedTable(EntityTypeBuilder<Movie> builder)
    {
        builder.OwnsMany(x => x.DraftsVetoedIn, ds =>
        {
            ds.ToTable(nameof(TableNames.MoviesDraftsVetoedInIds));

            ds.WithOwner()
                .HasForeignKey(ValueObjectNames.MovieId);

            ds.Property(x => x.Value)
                .HasColumnName(ColumnNames.DraftsVetoedInId)
                .ValueGeneratedNever();
        });

        builder.Metadata.FindNavigation(nameof(Movie.DraftsVetoedIn))
                        .SetPropertyAccessMode(PropertyAccessMode.Field);
    }

    private static void ConfigureMoviesDraftsSelectedTable(EntityTypeBuilder<Movie> builder)
    {
        builder.OwnsMany(x => x.DraftsSelectedIn, ds =>
        {
            ds.ToTable(nameof(TableNames.MoviesDraftsSelectedInIds));

            ds.WithOwner()
                .HasForeignKey(ValueObjectNames.MovieId);

            ds.Property(x => x.Value)
                .HasColumnName(ColumnNames.DraftsSelectedInId)
                .ValueGeneratedNever();
        });

        builder.Metadata.FindNavigation(nameof(Movie.DraftsSelectedIn))
                        .SetPropertyAccessMode(PropertyAccessMode.Field);
    }

    private static void ConfigureMoviesTable(EntityTypeBuilder<Movie> builder)
    {
        builder.ToTable(SchemaNames.Movies)
            .IsMultiTenant();

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Title)
                    .HasMaxLength(200);

        builder.Property(x => x.ImageUrl)
               .HasMaxLength(255);

        builder.Property(x => x.ImdbUrl)
            .HasMaxLength(255);
    }
}
