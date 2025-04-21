namespace ScreenDrafts.Modules.Integrations.Application.Movies.GetOnlineMovie;

public sealed record GetOnlineMovieCommand(string ImdbId) : ICommand<MovieResponse>;
