namespace ScreenDrafts.Modules.Reporting.Infrastructure.Movies;

internal sealed class MovieCanonicalPickConfiguration : IEntityTypeConfiguration<MovieCanonicalPick>
{
  public void Configure(EntityTypeBuilder<MovieCanonicalPick> builder)
  {
    builder.ToTable(Tables.MovieCanonicalPicks);

    builder.HasKey(x => x.Id);

    builder.Property(x => x.Id)
      .ValueGeneratedNever()
      .HasConversion(
        id => id.Value,
        value => MovieCanonicalPickId.Create(value));

    builder.Property(x => x.MoviePublicId)
      .IsRequired()
      .HasMaxLength(PublicIdPrefixes.MaxPublicIdLength);

    builder.Property(x => x.DraftPartPublicId)
      .IsRequired()
      .HasMaxLength(PublicIdPrefixes.MaxPublicIdLength);

    builder.HasIndex(x => new { x.MoviePublicId, x.DraftPartPublicId })
      .IsUnique()
      .HasDatabaseName("ux_movie_canonical_picks_movie_public_id_part_public_id");

    builder.HasIndex(x => x.MoviePublicId)
      .HasDatabaseName("ix_movie_canonical_picks_movie_public_id");

    builder.Property(x => x.BoardPosition)
      .IsRequired();

    builder.Property(x => x.PickedAt)
      .IsRequired();
  }
}
