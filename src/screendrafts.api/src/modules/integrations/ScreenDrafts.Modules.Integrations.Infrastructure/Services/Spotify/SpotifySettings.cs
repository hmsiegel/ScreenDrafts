namespace ScreenDrafts.Modules.Integrations.Infrastructure.Services.Spotify;

internal sealed class SpotifySettings
{
  public const string SectionName = "Integrations:Spotify";

  public string ClientId { get; set; } = default!;
  public string ClientSecret { get; set; } = default!;
  public string ShowId { get; set; } = default!;
  public string Market { get; set; } = "US";
  public string BaseAddress { get; set; } = "https://api.spotify.com/v1/";
  public string TokenUrl { get; set; } = "https://accounts.spotify.com/api/token";
}
