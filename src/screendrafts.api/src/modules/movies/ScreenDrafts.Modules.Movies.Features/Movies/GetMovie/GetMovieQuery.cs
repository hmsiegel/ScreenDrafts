namespace ScreenDrafts.Modules.Movies.Features.Movies.GetMovie;

internal sealed record GetMovieQuery(string ImdbId) : IQuery<MovieResponse>;
