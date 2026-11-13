namespace ScreenDrafts.Modules.Movies.Features.Movies.ResyncMediaPeople;

/// <summary>
/// Which titles still need their people synced. Idempotent: a title drops out of the list as
/// soon as one of its actors has a billing order, so a stopped run resumes where it left off.
/// </summary>
internal static class MediaPeopleResyncSql
{
  // Games and music videos have no TMDb cast. Titles without a TMDb id cannot be synced.
  private const string Where = """
    m.media_type NOT IN (@VideoGame, @MusicVideo)
    AND m.tmdb_id IS NOT NULL
    AND (
      -- Nothing attached yet.
      (
        NOT EXISTS (SELECT 1 FROM movies.media_directors md WHERE md.media_id = m.id)
        AND NOT EXISTS (SELECT 1 FROM movies.media_actors ma WHERE ma.media_id = m.id)
        AND NOT EXISTS (SELECT 1 FROM movies.media_production_companies mpc WHERE mpc.media_id = m.id)
      )
      -- Cast imported before billing order existed and not yet resynced. Once any actor is
      -- billed the title is done: leftover unbilled rows are IMDb-only cast TMDb does not
      -- list, and would otherwise re-select the title on every run.
      OR (
        EXISTS (SELECT 1 FROM movies.media_actors ma WHERE ma.media_id = m.id)
        AND NOT EXISTS (
          SELECT 1 FROM movies.media_actors ma
          WHERE ma.media_id = m.id AND ma.billing_order IS NOT NULL)
      )
    )
    """;

  internal const string Candidates = $"""
    SELECT m.public_id          AS PublicId,
           m.tmdb_id            AS TmdbId,
           m.imdb_id            AS ImdbId,
           m.media_type         AS MediaTypeValue,
           m.tv_series_tmdb_id  AS TvSeriesTmdbId,
           m.season_number      AS SeasonNumber,
           m.episode_number     AS EpisodeNumber
    FROM movies.media m
    WHERE {Where}
    ORDER BY m.public_id
    """;

  internal const string Count = $"""
    SELECT COUNT(*)::int
    FROM movies.media m
    WHERE {Where}
    """;
}
