namespace ScreenDrafts.Modules.Reporting.Infrastructure.Movies;

internal sealed class MovieHonorificsHistoryConfiguration : IEntityTypeConfiguration<MovieHonorificHistory>
{
  public void Configure(EntityTypeBuilder<MovieHonorificHistory> builder)
  {
    builder.ToTable(Tables.MoviesHonorificsHistory);

    builder.HasKey(x => x.Id);

    builder.Property(x => x.Id)
      .ValueGeneratedNever()
      .HasConversion(IdConverters.MovieHonorificHistoryIdConverter);

    builder.Property(x => x.MoviePublicId)
      .IsRequired()
      .HasMaxLength(PublicIdPrefixes.MaxPublicIdLength);

    builder.HasIndex(x => new { x.MoviePublicId, x.AchievedAt })
      .HasDatabaseName("ix_movie_honorifics_history_movie_public_id_achieved_at");

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

    builder.Property(x => x.AchievedAt)
      .IsRequired();
  }
}
