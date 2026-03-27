namespace ScreenDrafts.Modules.Reporting.Infrastructure.Movies;

internal sealed class MovieHonorificsConfiguration : IEntityTypeConfiguration<MovieHonorificEntity>
{
  public void Configure(EntityTypeBuilder<MovieHonorificEntity> builder)
  {
    builder.ToTable(Tables.MovieHonorifics);

    builder.HasKey(x => x.Id);

    builder.Property(x => x.Id)
      .ValueGeneratedNever()
      .HasConversion(IdConverters.MovieHonorificIdConverter);

    builder.Property(x => x.MoviePublicId)
      .IsRequired()
      .HasMaxLength(PublicIdPrefixes.MaxPublicIdLength);

    builder.HasIndex(x => x.MoviePublicId)
      .IsUnique()
      .HasDatabaseName("ux_movie_honorifics_movie_public_id");

    builder.Property(x => x.MovieTitle)
      .IsRequired()
      .HasColumnType("text");

    builder.Property(x => x.AppearanceHonorific)
      .IsRequired()
      .HasConversion(
        h => h.Value,
        value => MovieHonorific.FromValue(value));

    builder.Property(x => x.PositionHonorific)
      .IsRequired()
      .HasConversion(
        h => (int)h,
        value => (MoviePositionHonorific)value);

    builder.Property(x => x.AppearanceCount)
      .IsRequired();

    builder.Property(x => x.UpdateAtUtc)
      .IsRequired();
  }
}
