namespace ScreenDrafts.Modules.Integrations.Domain.Services.Igdb;

public interface IIgdbService
{
  Task<IgdbGameDetails?> GetGameDetailsAsync(int igdbId, CancellationToken cancellationToken = default);
}
