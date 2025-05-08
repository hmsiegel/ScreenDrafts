namespace ScreenDrafts.Seeding.Movies.Imdb.GetOnlineMovie;

public sealed record GetOnlineMovieCommand(string ImdbId) : ICommand<MovieResponse>;
