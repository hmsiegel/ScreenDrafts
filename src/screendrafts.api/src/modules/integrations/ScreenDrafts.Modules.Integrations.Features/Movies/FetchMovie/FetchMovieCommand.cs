namespace ScreenDrafts.Modules.Integrations.Features.Movies.FetchMovie;

internal sealed record FetchMovieCommand(string ImdbId) : ICommand;
