using ScreenDrafts.Modules.GuestDrafts.Domain.Drafts.Entities;

namespace ScreenDrafts.Modules.GuestDrafts.Infrastructure.GuestDrafts;

internal sealed class GuestDraftMovieConfiguration : IEntityTypeConfiguration<Movie>
{
  public void Configure(EntityTypeBuilder<Movie> builder)
  {
    builder.ToTable(Tables.GuestDraftMovies);

    builder.HasKey(x => x.Id);

    builder.Property(x => x.Id).ValueGeneratedNever();

    builder.Property(x => x.PublicId).IsRequired().HasMaxLength(PublicIdPrefixes.MaxPublicIdLength);

    builder.HasIndex(x => x.PublicId).IsUnique();

    builder.Property(x => x.MovieTitle).IsRequired();

    builder.Property(x => x.ImdbId);

    builder.Property(x => x.TmdbId);

    builder.Property(x => x.Year).HasMaxLength(10);

    builder.HasIndex(x => x.TmdbId).IsUnique().HasFilter("tmdb_id IS NOT NULL");

    // TV episode fields — only populated when MediaType is TvEpisode.
    builder.Property(x => x.TvSeriesTmdbId);

    builder.Property(x => x.SeasonNumber);

    builder.Property(x => x.EpisodeNumber);

    builder.Property(x => x.TvSeriesTitle).HasMaxLength(200);
  }
}
