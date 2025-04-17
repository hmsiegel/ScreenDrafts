namespace ScreenDrafts.Modules.Movies.Application.Movies.Queries.GetMovie;

public sealed record GetMovieQuery(string ImdbId) : IQuery<MovieResponse>;
