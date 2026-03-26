namespace ScreenDrafts.Modules.Integrations.Infrastructure.Services.Igdb;

internal sealed class IgdbSettings
{
  public const string SectionName = "Integrations:Igdb";

  public string ClientId { get; set; } = default!;
  public string ClientSecret { get; set; } = default!;
  public string BaseAddress { get; set; } = "https://api.igdb.com/v4/";
  public string TwitchTokenUrl { get; set; } = "https://id.twitch.tv/oauth2/token";
  public string Endpoint { get; set; } = "games";
}
