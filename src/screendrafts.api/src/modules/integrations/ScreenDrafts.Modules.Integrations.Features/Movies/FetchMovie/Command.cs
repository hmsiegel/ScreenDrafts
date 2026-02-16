namespace ScreenDrafts.Modules.Integrations.Features.Movies.FetchMovie;

internal sealed record Command(string ImdbId) : ICommand;
