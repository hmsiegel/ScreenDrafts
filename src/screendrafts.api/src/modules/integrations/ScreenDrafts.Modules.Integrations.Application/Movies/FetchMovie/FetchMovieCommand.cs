namespace ScreenDrafts.Modules.Integrations.Application.Movies.FetchMovie;

public sealed record FetchMovieCommand(string ImdbId) : ICommand;
