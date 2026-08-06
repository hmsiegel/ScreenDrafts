namespace ScreenDrafts.Modules.Integrations.Domain.Services.Igdb;

public interface IIgdbService
{
  Task<IgdbGameDetails?> GetGameDetailsAsync(
    int igdbId,
    CancellationToken cancellationToken = default
  );
  Task<IReadOnlyList<IgdbGameDetails>> SearchGamesAsync(
    string query,
    int page = 1,
    CancellationToken cancellationToken = default
  );
}
