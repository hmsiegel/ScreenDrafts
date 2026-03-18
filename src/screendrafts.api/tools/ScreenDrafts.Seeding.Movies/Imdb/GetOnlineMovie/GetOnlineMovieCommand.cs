namespace ScreenDrafts.Seeding.Movies.Imdb.GetOnlineMovie;

public sealed record GetOnlineMovieCommand(int TmdbId) : ICommand<MovieResponse>;
