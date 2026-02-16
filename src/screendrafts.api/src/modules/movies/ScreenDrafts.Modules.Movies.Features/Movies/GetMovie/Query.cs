namespace ScreenDrafts.Modules.Movies.Features.Movies.GetMovie;

internal sealed record Query(string ImdbId) : IQuery<MovieResponse>;
