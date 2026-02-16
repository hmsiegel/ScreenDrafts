namespace ScreenDrafts.Modules.Integrations.Features.Movies.GetOnlineMovie;

internal sealed record Command(string ImdbId) : ICommand<Response>;
