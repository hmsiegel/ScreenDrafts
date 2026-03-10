namespace ScreenDrafts.Modules.Integrations.Features.Movies.GetOnlineMovie;

internal sealed record GetOnlineMovieCommand(string ImdbId) : ICommand<GetOnlineMovieResponse>;
